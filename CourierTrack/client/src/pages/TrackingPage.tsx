import React, { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { trackingAPI } from '@/services/api'
import { Order, DeliveryStatus } from '@/types'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import MapComponent from '@/components/common/MapComponent'
import { Package, Phone, MapPin, Clock } from 'lucide-react'

const TrackingPage: React.FC = () => {
  const { trackingNumber } = useParams()
  const [order, setOrder] = useState<Order | null>(null)
  const [deliveryStatus, setDeliveryStatus] = useState<DeliveryStatus | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const fetchTracking = async () => {
      if (!trackingNumber) return

      setLoading(true)
      try {
        const response = await trackingAPI.trackByNumber(trackingNumber)
        setOrder(response.data.order)
        setDeliveryStatus(response.data.deliveryStatus)
      } catch (err: any) {
        setError('Sipariş bulunamadı')
      } finally {
        setLoading(false)
      }
    }

    fetchTracking()
  }, [trackingNumber])

  if (loading) return <LoadingSpinner message="Sipariş bilgisi alınıyor..." />

  if (error || !order) {
    return (
      <div className="max-w-2xl mx-auto py-12">
        <div className="bg-red-50 border border-red-200 rounded-lg p-8 text-center">
          <h2 className="text-2xl font-bold text-red-800 mb-2">Sipariş Bulunamadı</h2>
          <p className="text-red-600">{error}</p>
        </div>
      </div>
    )
  }

  const statusSteps = [
    { status: 'PENDING', label: 'Bekleniyor', icon: '📋' },
    { status: 'CONFIRMED', label: 'Onaylandı', icon: '✓' },
    { status: 'PICKED_UP', label: 'Alındı', icon: '📦' },
    { status: 'IN_TRANSIT', label: 'Yolda', icon: '🚚' },
    { status: 'DELIVERED', label: 'Teslim Edildi', icon: '✓' },
  ]

  const currentStepIndex = statusSteps.findIndex((step) => step.status === order.status)

  return (
    <div className="max-w-4xl mx-auto py-12 space-y-8">
      {/* Header */}
      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h1 className="text-3xl font-bold text-slate-900 mb-4">Takip Bilgileri</h1>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <p className="text-sm text-slate-600">Takip Numarası</p>
            <p className="text-lg font-semibold text-slate-900">{order.trackingNumber}</p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Durum</p>
            <p className="text-lg font-semibold text-blue-600">
              {statusSteps.find((s) => s.status === order.status)?.label}
            </p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Tutar</p>
            <p className="text-lg font-semibold text-slate-900">₺{order.price}</p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Tarih</p>
            <p className="text-lg font-semibold text-slate-900">
              {new Date(order.createdAt).toLocaleDateString('tr-TR')}
            </p>
          </div>
        </div>
      </div>

      {/* Status Timeline */}
      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h2 className="text-xl font-bold text-slate-900 mb-6">Teslimat İlerleme Durumu</h2>
        <div className="flex items-center justify-between">
          {statusSteps.map((step, index) => (
            <div key={step.status} className="flex flex-col items-center flex-1">
              <div
                className={`w-10 h-10 rounded-full flex items-center justify-center font-bold text-white mb-2 ${
                  index <= currentStepIndex
                    ? 'bg-blue-600'
                    : 'bg-slate-300'
                }`}
              >
                {step.icon}
              </div>
              <p className="text-sm text-slate-600 text-center">{step.label}</p>
              {index < statusSteps.length - 1 && (
                <div
                  className={`h-1 flex-1 my-2 ${
                    index < currentStepIndex ? 'bg-blue-600' : 'bg-slate-300'
                  }`}
                  style={{ minWidth: '20px' }}
                />
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Map */}
      {deliveryStatus && (
        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h2 className="text-xl font-bold text-slate-900 mb-4">Kurye Konumu</h2>
          <MapComponent
            center={{
              lat: deliveryStatus.currentLocation.latitude,
              lng: deliveryStatus.currentLocation.longitude,
            }}
            markers={[
              {
                position: {
                  lat: order.pickupAddress.latitude,
                  lng: order.pickupAddress.longitude,
                },
                title: 'Alım Noktası',
                color: 'green',
                infoContent: order.pickupAddress.street || 'Alım Noktası',
              },
              {
                position: {
                  lat: deliveryStatus.currentLocation.latitude,
                  lng: deliveryStatus.currentLocation.longitude,
                },
                title: 'Kurye Konumu',
                color: 'blue',
                infoContent: 'Kurye konumu',
              },
              {
                position: {
                  lat: order.deliveryAddress.latitude,
                  lng: order.deliveryAddress.longitude,
                },
                title: 'Teslimat Noktası',
                color: 'red',
                infoContent: order.deliveryAddress.street || 'Teslimat Noktası',
              },
            ]}
            zoom={14}
            height="400px"
          />
        </div>
      )}

      {/* Addresses */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h3 className="text-lg font-semibold text-slate-900 mb-4 flex items-center gap-2">
            <MapPin className="w-5 h-5 text-green-600" />
            Alım Adresi
          </h3>
          <p className="text-slate-600">
            {order.pickupAddress.street}, {order.pickupAddress.city}
          </p>
          <p className="text-slate-500 text-sm mt-2">
            {order.pickupAddress.zipCode} {order.pickupAddress.state}
          </p>
        </div>

        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h3 className="text-lg font-semibold text-slate-900 mb-4 flex items-center gap-2">
            <Package className="w-5 h-5 text-red-600" />
            Teslimat Adresi
          </h3>
          <p className="text-slate-600">
            {order.deliveryAddress.street}, {order.deliveryAddress.city}
          </p>
          <p className="text-slate-500 text-sm mt-2">
            {order.deliveryAddress.zipCode} {order.deliveryAddress.state}
          </p>
        </div>
      </div>

      {/* Package Details */}
      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h3 className="text-lg font-semibold text-slate-900 mb-4">Paket Bilgileri</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <p className="text-sm text-slate-600">Ağırlık</p>
            <p className="text-lg font-semibold text-slate-900">{order.package.weight} kg</p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Açıklama</p>
            <p className="text-lg font-semibold text-slate-900">{order.package.description}</p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Kırılgan</p>
            <p className="text-lg font-semibold text-slate-900">
              {order.package.fragile ? 'Evet' : 'Hayır'}
            </p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Değer</p>
            <p className="text-lg font-semibold text-slate-900">₺{order.package.value}</p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default TrackingPage
