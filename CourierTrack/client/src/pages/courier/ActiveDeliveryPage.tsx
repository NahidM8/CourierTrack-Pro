import React, { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { ordersAPI } from '@/services/api'
import { Order } from '@/types'
import MapComponent from '@/components/common/MapComponent'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import { courierAPI } from '@/services/api'
import { useAuth } from '@/context/AuthContext'
import { MapPin, Navigation, CheckCircle } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'

interface ActiveDeliveryPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const ActiveDeliveryPage: React.FC<ActiveDeliveryPageProps> = ({ onAddToast }) => {
  const { user } = useAuth()
  const { orderId } = useParams()
  const [order, setOrder] = useState<Order | null>(null)
  const [currentLocation, setCurrentLocation] = useState({ lat: 41.0082, lng: 28.9784 })
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchOrder()
    startLocationTracking()
  }, [orderId])

  const fetchOrder = async () => {
    if (!orderId) return
    setLoading(true)
    try {
      const response = await ordersAPI.getById(orderId)
      setOrder(response.data.order)
    } catch (err) {
      onAddToast?.('Sipariş yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const startLocationTracking = () => {
    if ('geolocation' in navigator) {
      const watchId = navigator.geolocation.watchPosition(
        (position) => {
          const { latitude, longitude } = position.coords
          setCurrentLocation({ lat: latitude, lng: longitude })

          if (user?.id) {
            courierAPI.updateLocation(user.id, latitude, longitude).catch((err) => {
              console.error('Failed to update courier location:', err)
            })
          }
        },
        (error) => {
          console.error('Geolocation error:', error)
          onAddToast?.('Konum alınamadı', 'error')
        },
        { enableHighAccuracy: true, maximumAge: 5000 }
      )

      return () => navigator.geolocation.clearWatch(watchId)
    }
  }

  const handlePickup = async () => {
    if (!order || !user?.id) return

    try {
      await ordersAPI.updateStatus(order.id, {
        courierId: user.id,
        status: 3,
        pickedUpAt: new Date().toISOString(),
        deliveredAt: null,
        note: 'Package picked up',
      })
      setOrder({ ...order, status: 'PICKED_UP' })
      onAddToast?.('Paket alındı olarak işaretlendi', 'success')
    } catch {
      onAddToast?.('Durum güncellenemedi', 'error')
    }
  }

  const handleDelivered = async () => {
    if (!order || !user?.id) return

    try {
      await ordersAPI.updateStatus(order.id, {
        courierId: user.id,
        status: 5,
        pickedUpAt: order.pickedUpAt ?? new Date().toISOString(),
        deliveredAt: new Date().toISOString(),
        note: 'Package delivered',
      })
      setOrder({ ...order, status: 'DELIVERED' })
      onAddToast?.('Paket teslim edildi', 'success')
    } catch {
      onAddToast?.('Durum güncellenemedi', 'error')
    }
  }

  if (loading) return <LoadingSpinner message="Sipariş yükleniyor..." />

  if (!order) return <div>Sipariş bulunamadı</div>

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Aktif Teslimat</h1>
        <p className="text-slate-600">Sipariş #{order.trackingNumber}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Map */}
        <div className="lg:col-span-2 bg-white rounded-lg border border-slate-200 overflow-hidden">
          <MapComponent
            center={currentLocation}
            zoom={15}
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
                position: currentLocation,
                title: 'Mevcut Konum',
                color: 'blue',
                infoContent: 'Mevcut konum',
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
            interactive={false}
          />
        </div>

        {/* Actions Panel */}
        <div className="space-y-4">
          {/* Status */}
          <div className="bg-white rounded-lg border border-slate-200 p-6">
            <h3 className="text-lg font-semibold text-slate-900 mb-4">Durum</h3>
            <p className={`text-lg font-bold px-4 py-2 rounded-lg text-center ${
              order.status === 'IN_TRANSIT'
                ? 'bg-orange-100 text-orange-800'
                : order.status === 'PICKED_UP'
                ? 'bg-blue-100 text-blue-800'
                : 'bg-green-100 text-green-800'
            }`}>
              {order.status === 'IN_TRANSIT' && 'Yolda'}
              {order.status === 'PICKED_UP' && 'Alındı'}
              {order.status === 'DELIVERED' && 'Teslim Edildi'}
            </p>
          </div>

          {/* Addresses */}
          <div className="bg-white rounded-lg border border-slate-200 p-6 space-y-3">
            <div>
              <p className="text-sm text-slate-600 flex items-center gap-1 mb-1">
                <MapPin className="w-4 h-4" />
                Alım Noktası
              </p>
              <p className="font-semibold text-slate-900">{order.pickupAddress.street}</p>
              <p className="text-sm text-slate-600">{order.pickupAddress.city}</p>
            </div>
            <div className="border-t border-slate-200 pt-3">
              <p className="text-sm text-slate-600 flex items-center gap-1 mb-1">
                <MapPin className="w-4 h-4" />
                Teslimat Noktası
              </p>
              <p className="font-semibold text-slate-900">{order.deliveryAddress.street}</p>
              <p className="text-sm text-slate-600">{order.deliveryAddress.city}</p>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="space-y-2">
            {order.status !== 'PICKED_UP' && order.status !== 'DELIVERED' && (
              <button
                onClick={handlePickup}
                className="w-full py-2 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors flex items-center justify-center gap-2"
              >
                <Navigation className="w-4 h-4" />
                Paket Alındı
              </button>
            )}
            {order.status === 'PICKED_UP' && order.status !== 'DELIVERED' && (
              <button
                onClick={handleDelivered}
                className="w-full py-2 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors flex items-center justify-center gap-2"
              >
                <CheckCircle className="w-4 h-4" />
                Teslim Edildi
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Package Details */}
      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-4">Paket Bilgileri</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <p className="text-sm text-slate-600">Ağırlık</p>
            <p className="text-lg font-semibold">{order.package.weight} kg</p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Açıklama</p>
            <p className="text-lg font-semibold">{order.package.description || '-'}</p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Kırılgan</p>
            <p className="text-lg font-semibold">{order.package.fragile ? 'Evet' : 'Hayır'}</p>
          </div>
          <div>
            <p className="text-sm text-slate-600">Değer</p>
            <p className="text-lg font-semibold">₺{order.package.value}</p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ActiveDeliveryPage
