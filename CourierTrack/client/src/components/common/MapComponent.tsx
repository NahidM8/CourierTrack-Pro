import React, { useEffect, useRef, useState } from 'react'
import { RouteInfo } from '@/types'
import { loadGoogleMaps } from '@/lib/googleMaps'

interface MapComponentProps {
  center?: { lat: number; lng: number }
  zoom?: number
  markers?: Array<{
    position: { lat: number; lng: number }
    title?: string
    color?: string
    iconUrl?: string
    infoContent?: string
  }>
  onLocationClick?: (lat: number, lng: number) => void
  height?: string
  className?: string
  polyline?: string
  route?: {
    origin: { lat: number; lng: number }
    destination: { lat: number; lng: number }
  }
  onRouteCalculated?: (info: RouteInfo) => void
  fitBounds?: boolean
  interactive?: boolean
}

const MapComponent: React.FC<MapComponentProps> = ({
  center = { lat: 41.0082, lng: 28.9784 }, // Istanbul default
  zoom = 13,
  markers = [],
  onLocationClick,
  height = '400px',
  className = '',
  polyline,
  route,
  onRouteCalculated,
  fitBounds = false,
  interactive = true,
}) => {
  const mapRef = useRef<HTMLDivElement>(null)
  const mapInstanceRef = useRef<google.maps.Map | null>(null)
  const markersRef = useRef<google.maps.Marker[]>([])
  const infoWindowRef = useRef<google.maps.InfoWindow | null>(null)
  const polylineRef = useRef<google.maps.Polyline | null>(null)
  const directionsRendererRef = useRef<google.maps.DirectionsRenderer | null>(null)
  const [isLoaded, setIsLoaded] = useState(false)

  useEffect(() => {
    let isMounted = true

    loadGoogleMaps()
      .then(() => {
        if (isMounted) {
          setIsLoaded(true)
        }
      })
      .catch((error) => {
        console.error('[MapComponent] Failed to load Google Maps:', error)
      })

    return () => {
      isMounted = false
    }
  }, [])

  useEffect(() => {
    if (!isLoaded || !mapRef.current || mapInstanceRef.current) return

    const newMap = new google.maps.Map(mapRef.current, {
      center,
      zoom,
      disableDefaultUI: false,
      clickableIcons: true,
    })

    mapInstanceRef.current = newMap
    infoWindowRef.current = new google.maps.InfoWindow()

    return () => {
      google.maps.event.clearInstanceListeners(newMap)
    }
  }, [center, zoom, isLoaded])

  useEffect(() => {
    const map = mapInstanceRef.current
    if (!map) return

    map.setCenter(center)
    map.setZoom(zoom)
  }, [center, zoom, isLoaded])

  useEffect(() => {
    const map = mapInstanceRef.current
    if (!map) return

    google.maps.event.clearListeners(map, 'click')
    if (interactive && onLocationClick) {
      map.addListener('click', (e: google.maps.MapMouseEvent) => {
        onLocationClick(e.latLng!.lat(), e.latLng!.lng())
      })
    }
  }, [interactive, onLocationClick, isLoaded])

  // Add markers
  useEffect(() => {
    const map = mapInstanceRef.current
    if (!map) return

    markersRef.current.forEach((marker) => marker.setMap(null))
    markersRef.current = markers.map((marker) => {
      const icon = marker.iconUrl
        ? marker.iconUrl
        : marker.color
          ? `http://maps.google.com/mapfiles/ms/icons/${marker.color}-dot.png`
          : undefined

      const mapMarker = new google.maps.Marker({
        position: marker.position,
        map,
        title: marker.title,
        icon,
      })

      if (marker.infoContent && infoWindowRef.current) {
        mapMarker.addListener('click', () => {
          infoWindowRef.current!.setContent(marker.infoContent || '')
          infoWindowRef.current!.open({ map, anchor: mapMarker })
        })
      }

      return mapMarker
    })

    if (fitBounds && markers.length > 0) {
      const bounds = new google.maps.LatLngBounds()
      markers.forEach((marker) => bounds.extend(marker.position))
      map.fitBounds(bounds)
    }
  }, [markers, fitBounds, isLoaded])

  // Draw polyline if provided
  useEffect(() => {
    const map = mapInstanceRef.current
    if (!map) return

    if (polylineRef.current) {
      polylineRef.current.setMap(null)
      polylineRef.current = null
    }

    if (!polyline || route) return

    const encodedPolyline = new google.maps.Polyline({
      map,
      path: google.maps.geometry.encoding.decodePath(polyline),
      geodesic: true,
      strokeColor: '#4F46E5',
      strokeOpacity: 0.7,
      strokeWeight: 3,
    })

    encodedPolyline.setMap(map)
    polylineRef.current = encodedPolyline
  }, [polyline, route, isLoaded])

  useEffect(() => {
    const map = mapInstanceRef.current
    if (!map) return

    if (!route) {
      if (directionsRendererRef.current) {
        directionsRendererRef.current.setMap(null)
        directionsRendererRef.current = null
      }
      return
    }

    const directionsService = new google.maps.DirectionsService()
    if (!directionsRendererRef.current) {
      directionsRendererRef.current = new google.maps.DirectionsRenderer({
        suppressMarkers: true,
        preserveViewport: !fitBounds,
        polylineOptions: {
          strokeColor: '#2563EB',
          strokeOpacity: 0.75,
          strokeWeight: 4,
        },
      })
      directionsRendererRef.current.setMap(map)
    }

    directionsService.route(
      {
        origin: route.origin,
        destination: route.destination,
        travelMode: google.maps.TravelMode.DRIVING,
      },
      (result, status) => {
        if (status !== google.maps.DirectionsStatus.OK || !result) {
          console.error('[MapComponent] Directions failed:', status)
          return
        }

        directionsRendererRef.current?.setDirections(result)

        const leg = result.routes[0]?.legs?.[0]
        if (leg && onRouteCalculated) {
          onRouteCalculated({
            distance: leg.distance?.text || '',
            duration: leg.duration?.text || '',
            polyline: result.routes[0]?.overview_polyline?.points || '',
          })
        }

        if (fitBounds && result.routes[0]?.bounds) {
          map.fitBounds(result.routes[0].bounds)
        }
      }
    )
  }, [route, onRouteCalculated, fitBounds, isLoaded])

  return (
    <div className={`w-full ${className}`} style={{ height }}>
      <div ref={mapRef} className="w-full h-full rounded-lg" />
    </div>
  )
}

export default MapComponent
