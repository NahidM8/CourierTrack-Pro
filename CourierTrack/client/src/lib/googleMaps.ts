const DEFAULT_LIBRARIES = ['places', 'geometry']

declare global {
  interface Window {
    __googleMapsPromise?: Promise<typeof google>
    __initGoogleMaps?: () => void
  }
}

export const loadGoogleMaps = (libraries: string[] = DEFAULT_LIBRARIES) => {
  if (typeof window === 'undefined') {
    return Promise.reject(new Error('Google Maps can only load in the browser'))
  }

  if (window.google?.maps) {
    return Promise.resolve(window.google)
  }

  if (window.__googleMapsPromise) {
    return window.__googleMapsPromise
  }

  const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY
  if (!apiKey) {
    return Promise.reject(new Error('Missing VITE_GOOGLE_MAPS_API_KEY'))
  }

  const url = new URL('https://maps.googleapis.com/maps/api/js')
  url.searchParams.set('key', apiKey)
  url.searchParams.set('libraries', libraries.join(','))
  url.searchParams.set('callback', '__initGoogleMaps')

  window.__googleMapsPromise = new Promise((resolve, reject) => {
    window.__initGoogleMaps = () => {
      resolve(window.google)
    }

    const script = document.createElement('script')
    script.src = url.toString()
    script.async = true
    script.defer = true
    script.onerror = () => reject(new Error('Failed to load Google Maps'))
    document.head.appendChild(script)
  })

  return window.__googleMapsPromise
}
