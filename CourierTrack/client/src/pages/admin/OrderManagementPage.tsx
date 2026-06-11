import React, { useState, useEffect } from 'react'
import { adminAPI } from '@/services/api'
import { Order, OrderStatus } from '@/types'
import { Package } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'

interface OrderManagementPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const OrderManagementPage: React.FC<OrderManagementPageProps> = ({ onAddToast }) => {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedStatus, setSelectedStatus] = useState<OrderStatus | 'ALL'>('ALL')

  useEffect(() => {
    fetchOrders()
  }, [selectedStatus])

  const fetchOrders = async () => {
    setLoading(true)
    try {
      const response = await adminAPI.getAllOrders(
        selectedStatus === 'ALL' ? undefined : selectedStatus
      )
      setOrders(response.data)
    } catch (err) {
      onAddToast?.('Siparişler yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const getStatusColor = (status: OrderStatus) => {
    const colors: Record<OrderStatus, string> = {
      PENDING: 'bg-yellow-100 text-yellow-800',
      CONFIRMED: 'bg-blue-100 text-blue-800',
      PICKED_UP: 'bg-purple-100 text-purple-800',
      IN_TRANSIT: 'bg-orange-100 text-orange-800',
      DELIVERED: 'bg-green-100 text-green-800',
      CANCELLED: 'bg-red-100 text-red-800',
    }
    return colors[status]
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Sipariş Yönetimi</h1>
        <p className="text-slate-600">Tüm siparişleri yönetin ve kontrol edin</p>
      </div>

      <div className="flex gap-2 mb-6 flex-wrap">
        {['ALL', 'PENDING', 'CONFIRMED', 'IN_TRANSIT', 'DELIVERED'].map((status) => (
          <button
            key={status}
            onClick={() => setSelectedStatus(status as any)}
            className={`px-4 py-2 rounded-lg font-medium transition-colors ${
              selectedStatus === status
                ? 'bg-blue-600 text-white'
                : 'bg-slate-200 text-slate-700 hover:bg-slate-300'
            }`}
          >
            {status}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Sipariş #</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Müşteri</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Kurye</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Tutar</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Durum</th>
                <th className="px-6 py-4 text-center text-sm font-semibold text-slate-900">İşlem</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {orders.map((order) => (
                <tr key={order.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-6 py-4">
                    <p className="font-semibold text-slate-900">{order.trackingNumber}</p>
                  </td>
                  <td className="px-6 py-4 text-slate-900">Müşteri Adı</td>
                  <td className="px-6 py-4 text-slate-900">{order.courierId || '-'}</td>
                  <td className="px-6 py-4 font-semibold text-blue-600">₺{order.price}</td>
                  <td className="px-6 py-4">
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(order.status)}`}>
                      {order.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-center">
                    <button className="text-blue-600 hover:text-blue-700 font-medium">
                      Detay
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default OrderManagementPage
