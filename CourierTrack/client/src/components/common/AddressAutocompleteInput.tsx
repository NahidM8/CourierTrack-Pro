import React, { useEffect, useRef } from 'react'
import { Address } from '@/types'
import { loadGoogleMaps } from '@/lib/googleMaps'

interface AddressAutocompleteInputProps {
  value: string
  placeholder?: string
  className?: string
  onChange: (value: string) => void
  onSelect: (address: Address) => void
}

const getComponent = (
  components: google.maps.GeocoderAddressComponent[] | undefined,
  types: string[]
) => {
  return components?.find((component) => types.every((type) => component.types.includes(type)))
}

const buildAddressFromPlace = (place: google.maps.places.PlaceResult): Address => {
  const location = place.geometry?.location
  const streetNumber = getComponent(place.address_components, ['street_number'])?.long_name
  const route = getComponent(place.address_components, ['route'])?.long_name
  const city =
    getComponent(place.address_components, ['locality'])?.long_name ||
    getComponent(place.address_components, ['administrative_area_level_2'])?.long_name ||
    ''
  const state = getComponent(place.address_components, ['administrative_area_level_1'])?.long_name || ''
  const zipCode = getComponent(place.address_components, ['postal_code'])?.long_name || ''
  const country = getComponent(place.address_components, ['country'])?.long_name || ''

  const street = [streetNumber, route].filter(Boolean).join(' ').trim() || place.formatted_address || ''

  return {
    id: '',
    street,
    city,
    state,
    zipCode,
    country,
    latitude: location?.lat() || 0,
    longitude: location?.lng() || 0,
  }
}

const AddressAutocompleteInput: React.FC<AddressAutocompleteInputProps> = ({
  value,
  placeholder,
  className,
  onChange,
  onSelect,
}) => {
  const inputRef = useRef<HTMLInputElement | null>(null)

  useEffect(() => {
    let autocomplete: google.maps.places.Autocomplete | null = null

    loadGoogleMaps(['places'])
      .then(() => {
        if (!inputRef.current) return

        autocomplete = new google.maps.places.Autocomplete(inputRef.current, {
          fields: ['address_components', 'geometry', 'formatted_address', 'name'],
          types: ['geocode'],
        })

        autocomplete.addListener('place_changed', () => {
          const place = autocomplete?.getPlace()
          if (!place || !place.geometry?.location) {
            return
          }

          const address = buildAddressFromPlace(place)
          onChange(place.formatted_address || address.street)
          onSelect(address)
        })
      })
      .catch((error) => {
        console.error('[AddressAutocompleteInput] Failed to load Google Maps:', error)
      })

    return () => {
      if (autocomplete) {
        google.maps.event.clearInstanceListeners(autocomplete)
      }
    }
  }, [onChange, onSelect])

  return (
    <input
      ref={inputRef}
      type="text"
      value={value}
      placeholder={placeholder}
      onChange={(event) => onChange(event.target.value)}
      className={className}
    />
  )
}

export default AddressAutocompleteInput
