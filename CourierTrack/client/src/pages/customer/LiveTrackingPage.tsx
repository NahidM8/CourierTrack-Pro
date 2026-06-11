import React, { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { ordersAPI } from '@/services/api'
import { Order, DeliveryStatus } from '@/types'
import MapComponent from '@/components/common/MapComponent'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import {
  initSignalRConnection,
  joinOrderGroup,
  leaveOrderGroup,
  onLocationUpdate,
  onOrderStatusChange,
} from '@/services/signalr'
import { MapPin, Truck, Phone } from 'lucide-react'

interface LiveTrackingPageProps {
  onAddToast?: (message: string, type: string) => void
}

const LiveTrackingPage: React.FC<LiveTrackingPageProps> = ({ onAddToast }) => {
  const { orderId } = useParams()
  const [order, setOrder] = useState<Order | null>(null)
  const [deliveryStatus, setDeliveryStatus] = useState<DeliveryStatus | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!orderId) return

    fetchOrderDetails()

    const setupRealtime = async () => {
      await initSignalRConnection()
      await joinOrderGroup(orderId)
    }

    setupRealtime().catch((err) => {
      console.error('Failed to join order tracking group:', err)
    })

    onLocationUpdate((data) => {
      const messageOrderId = data.orderId?.toString?.() ?? data.orderId
      if (messageOrderId !== orderId) return

      const location = data.currentLocation ?? {
        latitude: data.latitude,
        longitude: data.longitude,
      }

      setDeliveryStatus((prev) =>
        prev
          ? {
              ...prev,
              currentLocation: {
                latitude: location.latitude,
                longitude: location.longitude,
              },
            }
          : prev
      )
    })

    onOrderStatusChange((data) => {
      const messageOrderId = data.orderId?.toString?.() ?? data.orderId
      if (messageOrderId !== orderId) return

      setOrder((prev) =>
        prev ? { ...prev, status: data.status?.toUpperCase?.() ?? data.status } : null
      )
    })

    return () => {
      leaveOrderGroup(orderId).catch(() => undefined)
    }
  }, [orderId])

  const fetchOrderDetails = async () => {
    if (!orderId) return
    setLoading(true)
    try {
      const response = await ordersAPI.getById(orderId)
      setOrder(response.data.order)
      setDeliveryStatus(response.data.deliveryStatus)
    } catch (err) {
      console.error('Error fetching order:', err)
    } finally {
      setLoading(false)
    }
  }

  if (loading) return <LoadingSpinner message="Sipariş bilgisi yükleniyor..." />

  if (!order || !deliveryStatus) {
    return (
      <div className="text-center py-12">
        <p className="text-slate-600">Sipariş bulunamadı</p>
      </div>
    )
  }

  return (
    <div className="max-w-6xl mx-auto py-12 space-y-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Canlı Takip</h1>
        <p className="text-slate-600">Paketinizin konumunu gerçek zamanlı izleyin</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Map */}
        <div className="lg:col-span-2 bg-white rounded-lg border border-slate-200 overflow-hidden">
          <MapComponent
            center={{
              lat: deliveryStatus.currentLocation.latitude,
              lng: deliveryStatus.currentLocation.longitude,
            }}
            zoom={14}
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
            interactive={false}
          />
        </div>

        {/* Info Panel */}
        <div className="space-y-4">
          {/* Order Status */}
          <div className="bg-white rounded-lg border border-slate-200 p-6">
            <h3 className="text-lg font-semibold text-slate-900 mb-4">Sipariş Durumu</h3>
            <div className="space-y-3">
              <div>
                <p className="text-sm text-slate-600">Takip Numarası</p>
                <p className="text-lg font-semibold text-slate-900">{order.trackingNumber}</p>
              </div>
              <div>
                <p className="text-sm text-slate-600">Mevcut Durum</p>
                <p className="text-lg font-semibold text-blue-600 capitalize">
                  {order.status === 'IN_TRANSIT' ? 'Yolda' : order.status}
                </p>
              </div>
              <div>
                <p className="text-sm text-slate-600">Tahmini Teslimat</p>
                <p className="text-lg font-semibold text-slate-900">
                  {deliveryStatus.estimatedArrival
                    ? new Date(deliveryStatus.estimatedArrival).toLocaleTimeString('tr-TR', {
                      hour: '2-digit',
                      minute: '2-digit',
                    })
                    : '-'}
                </p>
              </div>
            </div>
          </div>

          {/* Courier Info */}
          <div className="bg-white rounded-lg border border-slate-200 p-6">
            <h3 className="text-lg font-semibold text-slate-900 mb-4 flex items-center gap-2">
              <Truck className="w-5 h-5" />
              Kurye Bilgileri
            </h3>
            <div className="space-y-3">
              <div>
                <p className="text-sm text-slate-600">Kurye Adı</p>
                <p className="text-lg font-semibold text-slate-900">-</p>
              </div>
              <div>
                <p className="text-sm text-slate-600">Telefon</p>
                <button className="text-blue-600 font-semibold hover:text-blue-700 flex items-center gap-1">
                  <Phone className="w-4 h-4" />
                  Ara
                </button>
              </div>
            </div>
          </div>

          {/* Addresses */}
          <div className="bg-white rounded-lg border border-slate-200 p-6 space-y-3">
            <div>
              <p className="text-sm text-slate-600 flex items-center gap-1 mb-1">
                <MapPin className="w-4 h-4" />
                Alım Noktası
              </p>
              <p className="text-slate-900">{order.pickupAddress.street}</p>
              <p className="text-sm text-slate-600">{order.pickupAddress.city}</p>
            </div>
            <div className="border-t border-slate-200 pt-3">
              <p className="text-sm text-slate-600 flex items-center gap-1 mb-1">
                <MapPin className="w-4 h-4" />
                Teslimat Noktası
              </p>
              <p className="text-slate-900">{order.deliveryAddress.street}</p>
              <p className="text-sm text-slate-600">{order.deliveryAddress.city}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default LiveTrackingPage
