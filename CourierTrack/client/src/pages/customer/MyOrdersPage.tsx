import React, { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { ordersAPI } from '@/services/api'
import { Order, OrderStatus } from '@/types'
import LoadingSpinner from '@/components/common/LoadingSpinner'
import { Package, MapPin, Clock } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'

interface MyOrdersPageProps {
  onAddToast: (message: string, type: ToastType) => void
}

const MyOrdersPage: React.FC<MyOrdersPageProps> = ({ onAddToast }) => {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedStatus, setSelectedStatus] = useState<OrderStatus | 'ALL'>('ALL')

  useEffect(() => {
    fetchOrders()
  }, [selectedStatus])

  const fetchOrders = async () => {
    setLoading(true)
    try {
      const response = await ordersAPI.getMyOrders(
        selectedStatus === 'ALL' ? undefined : selectedStatus
      )
      setOrders(response.data)
    } catch (err) {
      onAddToast('Siparişler yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const getStatusColor = (status: OrderStatus) => {
    const colors: Record<OrderStatus, string> = {
      CREATED: 'bg-slate-100 text-slate-700',
      PENDING: 'bg-yellow-100 text-yellow-800',
      ASSIGNED: 'bg-blue-100 text-blue-800',
      PICKED_UP: 'bg-blue-100 text-blue-800',
      IN_TRANSIT: 'bg-blue-100 text-blue-800',
      DELIVERED: 'bg-green-100 text-green-800',
      FAILED: 'bg-red-100 text-red-800',
      CANCELLED: 'bg-slate-200 text-slate-700',
    }
    return colors[status]
  }

  const getStatusLabel = (status: OrderStatus) => {
    const labels: Record<OrderStatus, string> = {
      CREATED: 'Oluşturuldu',
      PENDING: 'Beklemede',
      ASSIGNED: 'Atandı',
      PICKED_UP: 'Alındı',
      IN_TRANSIT: 'Yolda',
      DELIVERED: 'Teslim Edildi',
      FAILED: 'Başarısız',
      CANCELLED: 'İptal Edildi',
    }
    return labels[status]
  }

  if (loading) return <LoadingSpinner message="Siparişler yükleniyor..." />

  return (
    <div className="max-w-6xl mx-auto py-12">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Siparişlerim</h1>
        <p className="text-slate-600">Tüm siparişlerinizi burada görüntüleyin</p>
      </div>

      {/* Filter Buttons */}
      <div className="flex gap-2 mb-8 flex-wrap">
        {['ALL', 'PENDING', 'ASSIGNED', 'PICKED_UP', 'IN_TRANSIT', 'DELIVERED', 'FAILED', 'CANCELLED'].map((status) => (
          <button
            key={status}
            onClick={() => setSelectedStatus(status as any)}
            className={`px-4 py-2 rounded-lg font-medium transition-colors ${
              selectedStatus === status
                ? 'bg-blue-600 text-white'
                : 'bg-slate-200 text-slate-700 hover:bg-slate-300'
            }`}
          >
            {status === 'ALL' ? 'Tümü' : getStatusLabel(status as OrderStatus)}
          </button>
        ))}
      </div>

      {/* Orders List */}
      {orders.length === 0 ? (
        <div className="text-center py-12">
          <Package className="w-16 h-16 text-slate-300 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-slate-900 mb-2">Sipariş Bulunamadı</h3>
          <p className="text-slate-600 mb-4">Henüz hiç sipariş oluşturmadınız</p>
          <Link
            to="/customer/create-order"
            className="inline-block px-6 py-2 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700"
          >
            İlk Siparişinizi Oluşturun
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <div key={order.id} className="bg-white rounded-lg border border-slate-200 p-6 hover:shadow-md transition-shadow">
              <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="text-lg font-semibold text-slate-900">
                      Sipariş #{order.trackingNumber}
                    </h3>
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(order.status)}`}>
                      {getStatusLabel(order.status)}
                    </span>
                  </div>
                  <p className="text-slate-600 text-sm mb-2 flex items-center gap-1">
                    <MapPin className="w-4 h-4" />
                    {order.deliveryAddress.city} - {order.deliveryAddress.street}
                  </p>
                  <p className="text-slate-600 text-sm flex items-center gap-1">
                    <Clock className="w-4 h-4" />
                    {new Date(order.createdAt).toLocaleDateString('tr-TR')}
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-2xl font-bold text-blue-600 mb-4">₺{order.price}</p>
                  <div className="flex gap-2">
                    {order.status !== 'DELIVERED' && order.status !== 'FAILED' && order.status !== 'CANCELLED' && (
                      <Link
                        to={`/customer/live-tracking/${order.id}`}
                        className="px-4 py-2 bg-blue-100 text-blue-600 rounded-lg font-medium hover:bg-blue-200 transition-colors"
                      >
                        Takip Et
                      </Link>
                    )}
                    <Link
                      to={`/tracking/${order.trackingNumber}`}
                      className="px-4 py-2 bg-slate-100 text-slate-700 rounded-lg font-medium hover:bg-slate-200 transition-colors"
                    >
                      Detaylar
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

export default MyOrdersPage
