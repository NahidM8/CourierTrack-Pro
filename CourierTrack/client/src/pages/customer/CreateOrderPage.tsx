import React, { useEffect, useMemo, useState } from 'react'
import { CardElement, Elements, useElements, useStripe } from '@stripe/react-stripe-js'
import { loadStripe } from '@stripe/stripe-js'
import { ordersAPI, paymentAPI } from '@/services/api'
import MapComponent from '@/components/common/MapComponent'
import AddressAutocompleteInput from '@/components/common/AddressAutocompleteInput'
import { Package, DollarSign } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'
import { Address, RouteInfo } from '@/types'
import { loadGoogleMaps } from '@/lib/googleMaps'

interface CreateOrderPageProps {
  onAddToast: (message: string, type: ToastType) => void
}

interface PaymentFormProps {
  clientSecret: string
  onSuccess: () => void
  onFailure: (message: string) => void
}

const PaymentForm: React.FC<PaymentFormProps> = ({ clientSecret, onSuccess, onFailure }) => {
  const stripe = useStripe()
  const elements = useElements()
  const [processing, setProcessing] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage(null)

    if (!stripe || !elements) {
      setErrorMessage('Odeme sistemi hazir degil')
      return
    }

    const cardElement = elements.getElement(CardElement)
    if (!cardElement) {
      setErrorMessage('Kart bilgileri alinamadi')
      return
    }

    setProcessing(true)
    try {
      const result = await stripe.confirmCardPayment(clientSecret, {
        payment_method: { card: cardElement },
      })

      if (result.error) {
        const message = result.error.message || 'Odeme basarisiz'
        setErrorMessage(message)
        onFailure(message)
        return
      }

      if (result.paymentIntent?.status === 'succeeded') {
        onSuccess()
        return
      }

      setErrorMessage('Odeme tamamlanamadi')
      onFailure('Odeme tamamlanamadi')
    } finally {
      setProcessing(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="rounded-lg border border-slate-200 p-4">
        <CardElement
          options={{
            hidePostalCode: true,
            style: {
              base: {
                fontSize: '16px',
                color: '#0f172a',
                '::placeholder': { color: '#94a3b8' },
              },
            },
          }}
        />
      </div>
      {errorMessage && (
        <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">
          {errorMessage}
        </div>
      )}
      <button
        type="submit"
        disabled={!stripe || processing}
        className="w-full py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors disabled:opacity-50"
      >
        {processing ? 'Odeme isleniyor...' : 'Odeme Yap'}
      </button>
    </form>
  )
}

const CreateOrderPage: React.FC<CreateOrderPageProps> = ({ onAddToast }) => {
  const [step, setStep] = useState(1)
  const [formData, setFormData] = useState({
    pickupAddress: { street: '', city: '', state: '', zipCode: '', country: 'Türkiye', latitude: 41.0082, longitude: 28.9784 },
    deliveryAddress: { street: '', city: '', state: '', zipCode: '', country: 'Türkiye', latitude: 41.0082, longitude: 28.9784 },
    package: { weight: 1, dimensions: { length: 10, width: 10, height: 10 }, description: '', fragile: false, value: 100 },
  })
  const [routeInfo, setRouteInfo] = useState<RouteInfo | null>(null)
  const [price, setPrice] = useState(0)
  const [loading, setLoading] = useState(false)
  const [orderId, setOrderId] = useState<string | null>(null)
  const [clientSecret, setClientSecret] = useState<string | null>(null)
  const [paymentStatus, setPaymentStatus] = useState<'idle' | 'succeeded' | 'failed'>('idle')

  const stripeKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || ''
  const stripeReady = useMemo(() => Boolean(stripeKey), [stripeKey])
  const stripePromise = useMemo(
    () => (stripeKey ? loadStripe(stripeKey) : null),
    [stripeKey]
  )

  useEffect(() => {
    if (step !== 3 || clientSecret) {
      return
    }

    const initPayment = async () => {
      setLoading(true)
      try {
        const orderResponse = await ordersAPI.create({
          ...formData,
          price,
        })

        const createdOrderId =
          orderResponse.data?.id || orderResponse.data?.orderId || orderResponse.data?._id

        if (!createdOrderId) {
          throw new Error('Siparis kimligi bulunamadi')
        }

        setOrderId(createdOrderId)

        const paymentResponse = await paymentAPI.createPaymentIntent({
          orderId: createdOrderId,
          amount: price,
        })

        const secret =
          paymentResponse.data?.clientSecret || paymentResponse.data?.client_secret

        if (!secret) {
          throw new Error('Odeme gizli anahtari alinamadi')
        }

        setClientSecret(secret)
      } catch (err) {
        setPaymentStatus('failed')
        onAddToast('Odeme baslatilamadi', 'error')
      } finally {
        setLoading(false)
      }
    }

    initPayment()
  }, [clientSecret, formData, onAddToast, price, step])

  const handleCalculatePrice = async () => {
    setLoading(true)
    try {
      const response = await ordersAPI.calculatePrice(formData)
      setPrice(response.data.price)
      setStep(2)
      onAddToast('Fiyat hesaplandı', 'success')
    } catch (err) {
      onAddToast('Fiyat hesaplama başarısız', 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleStartPayment = () => {
    setPaymentStatus('idle')
    setClientSecret(null)
    setOrderId(null)
    setStep(3)
  }

  const updateAddress = (type: 'pickup' | 'delivery', address: Address) => {
    if (type === 'pickup') {
      setFormData((prev) => ({
        ...prev,
        pickupAddress: {
          ...prev.pickupAddress,
          ...address,
        },
      }))
    } else {
      setFormData((prev) => ({
        ...prev,
        deliveryAddress: {
          ...prev.deliveryAddress,
          ...address,
        },
      }))
    }
  }

  const reverseGeocode = async (lat: number, lng: number) => {
    const googleMaps = await loadGoogleMaps(['places'])
    const geocoder = new googleMaps.maps.Geocoder()

    return new Promise<Address>((resolve, reject) => {
      geocoder.geocode({ location: { lat, lng } }, (results, status) => {
        if (status !== googleMaps.maps.GeocoderStatus.OK || !results?.[0]) {
          reject(new Error('Geocoding failed'))
          return
        }

        const result = results[0]
        const getComponent = (types: string[]) =>
          result.address_components?.find((component) =>
            types.every((type) => component.types.includes(type))
          )

        const streetNumber = getComponent(['street_number'])?.long_name
        const route = getComponent(['route'])?.long_name
        const city =
          getComponent(['locality'])?.long_name ||
          getComponent(['administrative_area_level_2'])?.long_name ||
          ''
        const state = getComponent(['administrative_area_level_1'])?.long_name || ''
        const zipCode = getComponent(['postal_code'])?.long_name || ''
        const country = getComponent(['country'])?.long_name || ''
        const street = [streetNumber, route].filter(Boolean).join(' ').trim() || result.formatted_address || ''

        resolve({
          id: '',
          street,
          city,
          state,
          zipCode,
          country,
          latitude: lat,
          longitude: lng,
        })
      })
    })
  }

  const handleMapClick = async (type: 'pickup' | 'delivery', lat: number, lng: number) => {
    try {
      const address = await reverseGeocode(lat, lng)
      updateAddress(type, address)
    } catch {
      onAddToast('Adres bulunamadi', 'error')
    }
  }

  return (
    <div className="max-w-4xl mx-auto py-12">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Yeni Sipariş Oluştur</h1>
        <p className="text-slate-600">Aşağıdaki adımları takip ederek yeni bir sipariş oluşturun</p>
      </div>

      {/* Steps */}
      <div className="flex gap-4 mb-8">
        {[1, 2, 3].map((s) => (
          <div
            key={s}
            className={`flex-1 py-2 px-4 rounded-lg font-semibold text-center ${
              step >= s
                ? 'bg-blue-600 text-white'
                : 'bg-slate-200 text-slate-600'
            }`}
          >
            Adım {s}
          </div>
        ))}
      </div>

      {step === 1 && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Pickup Address */}
            <div className="bg-white rounded-lg border border-slate-200 p-6">
              <h2 className="text-xl font-semibold text-slate-900 mb-4">Alım Adresi</h2>
              <div className="space-y-4">
                <AddressAutocompleteInput
                  value={formData.pickupAddress.street}
                  placeholder="Sokak Adresi"
                  onChange={(value) => setFormData({
                    ...formData,
                    pickupAddress: { ...formData.pickupAddress, street: value },
                  })}
                  onSelect={(address) => updateAddress('pickup', address)}
                  className="w-full px-4 py-3 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-transparent transition-colors"
                />
                <input
                  type="text"
                  placeholder="Åžehir"
                  value={formData.pickupAddress.city}
                  onChange={(e) => setFormData({
                    ...formData,
                    pickupAddress: { ...formData.pickupAddress, city: e.target.value }
                  })}
                  className="w-full px-4 py-3 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-transparent transition-colors"
                />
                <input
                  type="text"
                  placeholder="Posta Kodu"
                  value={formData.pickupAddress.zipCode}
                  onChange={(e) => setFormData({
                    ...formData,
                    pickupAddress: { ...formData.pickupAddress, zipCode: e.target.value }
                  })}
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg"
                />
              </div>
              <div className="mt-4">
                <p className="text-sm text-slate-600 mb-2">Harita üzerinde konumu seçin</p>
                <MapComponent
                  height="300px"
                  center={{
                    lat: formData.pickupAddress.latitude,
                    lng: formData.pickupAddress.longitude,
                  }}
                  markers={[
                    {
                      position: {
                        lat: formData.pickupAddress.latitude,
                        lng: formData.pickupAddress.longitude,
                      },
                      title: 'Alım Noktası',
                      color: 'green',
                      infoContent: formData.pickupAddress.street || 'Alım Noktası',
                    },
                  ]}
                  onLocationClick={(lat, lng) => handleMapClick('pickup', lat, lng)}
                />
              </div>
            </div>

            {/* Delivery Address */}
            <div className="bg-white rounded-lg border border-slate-200 p-6">
              <h2 className="text-xl font-semibold text-slate-900 mb-4">Teslimat Adresi</h2>
              <div className="space-y-4">
                <AddressAutocompleteInput
                  value={formData.deliveryAddress.street}
                  placeholder="Sokak Adresi"
                  onChange={(value) => setFormData({
                    ...formData,
                    deliveryAddress: { ...formData.deliveryAddress, street: value },
                  })}
                  onSelect={(address) => updateAddress('delivery', address)}
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg"
                />
                <input
                  type="text"
                  placeholder="Şehir"
                  value={formData.deliveryAddress.city}
                  onChange={(e) => setFormData({
                    ...formData,
                    deliveryAddress: { ...formData.deliveryAddress, city: e.target.value }
                  })}
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg"
                />
                <input
                  type="text"
                  placeholder="Posta Kodu"
                  value={formData.deliveryAddress.zipCode}
                  onChange={(e) => setFormData({
                    ...formData,
                    deliveryAddress: { ...formData.deliveryAddress, zipCode: e.target.value }
                  })}
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg"
                />
              </div>
              <div className="mt-4">
                <p className="text-sm text-slate-600 mb-2">Harita üzerinde konumu seçin</p>
                <MapComponent
                  height="300px"
                  center={{
                    lat: formData.deliveryAddress.latitude,
                    lng: formData.deliveryAddress.longitude,
                  }}
                  markers={[
                    {
                      position: {
                        lat: formData.deliveryAddress.latitude,
                        lng: formData.deliveryAddress.longitude,
                      },
                      title: 'Teslimat Noktası',
                      color: 'red',
                      infoContent: formData.deliveryAddress.street || 'Teslimat Noktası',
                    },
                  ]}
                  onLocationClick={(lat, lng) => handleMapClick('delivery', lat, lng)}
                />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg border border-slate-200 p-6">
            <h2 className="text-xl font-semibold text-slate-900 mb-4">Rota Önizleme</h2>
            <MapComponent
              height="320px"
              route={{
                origin: {
                  lat: formData.pickupAddress.latitude,
                  lng: formData.pickupAddress.longitude,
                },
                destination: {
                  lat: formData.deliveryAddress.latitude,
                  lng: formData.deliveryAddress.longitude,
                },
              }}
              fitBounds
              markers={[
                {
                  position: {
                    lat: formData.pickupAddress.latitude,
                    lng: formData.pickupAddress.longitude,
                  },
                  title: 'Alım Noktası',
                  color: 'green',
                  infoContent: formData.pickupAddress.street || 'Alım Noktası',
                },
                {
                  position: {
                    lat: formData.deliveryAddress.latitude,
                    lng: formData.deliveryAddress.longitude,
                  },
                  title: 'Teslimat Noktası',
                  color: 'red',
                  infoContent: formData.deliveryAddress.street || 'Teslimat Noktası',
                },
              ]}
              onRouteCalculated={(info) => setRouteInfo(info)}
            />
            {routeInfo && (
              <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-600">
                <span>Mesafe: {routeInfo.distance || '-'}</span>
                <span>Süre: {routeInfo.duration || '-'}</span>
              </div>
            )}
          </div>

          {/* Package Details */}
          <div className="bg-white rounded-lg border border-slate-200 p-6">
            <h2 className="text-xl font-semibold text-slate-900 mb-4 flex items-center gap-2">
              <Package className="w-5 h-5" />
              Paket Bilgileri
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">Ağırlık (kg)</label>
                <input
                  type="number"
                  value={formData.package.weight}
                  onChange={(e) => setFormData({
                    ...formData,
                    package: { ...formData.package, weight: parseFloat(e.target.value) }
                  })}
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">Değer (₺)</label>
                <input
                  type="number"
                  value={formData.package.value}
                  onChange={(e) => setFormData({
                    ...formData,
                    package: { ...formData.package, value: parseFloat(e.target.value) }
                  })}
                  className="w-full px-4 py-2 border border-slate-300 rounded-lg"
                />
              </div>
            </div>
            <div className="mt-4">
              <label className="block text-sm font-medium text-slate-700 mb-2">Açıklama</label>
              <textarea
                value={formData.package.description}
                onChange={(e) => setFormData({
                  ...formData,
                  package: { ...formData.package, description: e.target.value }
                })}
                className="w-full px-4 py-2 border border-slate-300 rounded-lg"
                rows={3}
              />
            </div>
            <label className="flex items-center gap-2 mt-4">
              <input
                type="checkbox"
                checked={formData.package.fragile}
                onChange={(e) => setFormData({
                  ...formData,
                  package: { ...formData.package, fragile: e.target.checked }
                })}
                className="w-4 h-4 rounded"
              />
              <span className="text-slate-700">Bu paket kırılgan ürün içeriyor</span>
            </label>
          </div>

          <button
            onClick={handleCalculatePrice}
            disabled={loading}
            className="w-full py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors disabled:opacity-50"
          >
            {loading ? 'Hesaplanıyor...' : 'Fiyat Hesapla ve Devam Et'}
          </button>
        </div>
      )}

      {step === 2 && (
        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h2 className="text-2xl font-bold text-slate-900 mb-6 flex items-center gap-2">
            <DollarSign className="w-6 h-6" />
            Fiyat Özeti
          </h2>
          <div className="space-y-4 mb-6">
            <div className="flex justify-between text-lg">
              <span className="text-slate-600">Teslimat Ücreti:</span>
              <span className="font-semibold text-slate-900">₺{price}</span>
            </div>
            <div className="border-t border-slate-200 pt-4 flex justify-between text-2xl">
              <span className="font-semibold text-slate-900">Toplam:</span>
              <span className="font-bold text-blue-600">₺{price}</span>
            </div>
          </div>
          <div className="flex gap-4">
            <button
              onClick={() => setStep(1)}
              className="flex-1 py-3 border-2 border-blue-600 text-blue-600 rounded-lg font-semibold hover:bg-blue-50 transition-colors"
            >
              Geri
            </button>
            <button
              onClick={handleStartPayment}
              disabled={loading}
              className="flex-1 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {loading ? 'Hazirlaniyor...' : 'Ödemeye Devam Et'}
            </button>
          </div>
        </div>
      )}

      {step === 3 && (
        <div className="bg-white rounded-lg border border-slate-200 p-6 space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="text-2xl font-bold text-slate-900 flex items-center gap-2">
              <DollarSign className="w-6 h-6" />
              Odeme
            </h2>
            <button
              onClick={() => setStep(2)}
              className="text-sm font-semibold text-blue-600 hover:text-blue-700"
            >
              Geri don
            </button>
          </div>

          <div className="rounded-lg bg-slate-50 p-4">
            <div className="flex justify-between text-lg">
              <span className="text-slate-600">Toplam:</span>
              <span className="font-semibold text-slate-900">₺{price}</span>
            </div>
            {orderId && (
              <div className="mt-2 text-xs text-slate-500">Siparis No: {orderId}</div>
            )}
          </div>

          {!stripeReady && (
            <div className="rounded-lg bg-amber-50 px-4 py-3 text-sm text-amber-700">
              Stripe anahtari bulunamadi. VITE_STRIPE_PUBLISHABLE_KEY ayarini ekleyin.
            </div>
          )}

          <div className="rounded-lg bg-blue-50 px-4 py-3 text-sm text-blue-700">
            Test karti: 4242 4242 4242 4242, SKT: 12/34, CVC: 123
          </div>

          {loading && (
            <div className="rounded-lg bg-slate-100 px-4 py-3 text-sm text-slate-600">
              Odeme bilgileri hazirlaniyor...
            </div>
          )}

          {paymentStatus === 'succeeded' && (
            <div className="rounded-lg bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
              Odeme basarili. Siparisiniz alinmistir.
            </div>
          )}

          {paymentStatus === 'failed' && (
            <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">
              Odeme basarisiz. Lutfen tekrar deneyin.
            </div>
          )}

          {stripeReady && clientSecret && paymentStatus !== 'succeeded' && stripePromise && (
            <Elements stripe={stripePromise} options={{ clientSecret }}>
              <PaymentForm
                clientSecret={clientSecret}
                onSuccess={() => {
                  setPaymentStatus('succeeded')
                  onAddToast('Odeme basarili', 'success')
                }}
                onFailure={(message) => {
                  setPaymentStatus('failed')
                  onAddToast(message, 'error')
                }}
              />
            </Elements>
          )}
        </div>
      )}
    </div>
  )
}

export default CreateOrderPage
