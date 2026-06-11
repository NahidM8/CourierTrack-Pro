import React, { useState, useEffect } from 'react'
import { courierAPI, ordersAPI } from '@/services/api'
import { Order } from '@/types'
import { Package, MapPin } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'
import { useAuth } from '@/context/AuthContext'

interface IncomingOrdersPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const IncomingOrdersPage: React.FC<IncomingOrdersPageProps> = ({ onAddToast }) => {
  const { user } = useAuth()
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchIncomingOrders()
  }, [])

  const fetchIncomingOrders = async () => {
    setLoading(true)
    try {
      if (!user?.id) {
        onAddToast?.('Kurye bilgisi bulunamadı', 'error')
        return
      }

      const response = await courierAPI.getOrders(user.id)
      setOrders(response.data?.data || [])
    } catch (err) {
      onAddToast?.('Siparişler yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleAcceptOrder = async (orderId: string) => {
    try {
      if (!user?.id) {
        onAddToast?.('Kurye bilgisi bulunamadı', 'error')
        return
      }

      await ordersAPI.updateStatus(orderId, {
        courierId: user.id,
        status: 2,
        pickedUpAt: null,
        deliveredAt: null,
        note: 'Accepted by courier',
      })
      setOrders(orders.filter(o => o.id !== orderId))
      onAddToast?.('Sipariş kabul edildi', 'success')
    } catch (err) {
      onAddToast?.('İşlem başarısız', 'error')
    }
  }

  const handleRejectOrder = async (orderId: string) => {
    try {
      if (!user?.id) {
        onAddToast?.('Kurye bilgisi bulunamadı', 'error')
        return
      }

      await ordersAPI.updateStatus(orderId, {
        courierId: user.id,
        status: 1,
        pickedUpAt: null,
        deliveredAt: null,
        note: 'Rejected by courier',
      })
      setOrders(orders.filter(o => o.id !== orderId))
      onAddToast?.('Sipariş reddedildi', 'success')
    } catch (err) {
      onAddToast?.('İşlem başarısız', 'error')
    }
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Gelen Siparişler</h1>
        <p className="text-slate-600">Size atanan yeni siparişleri görüntüleyin ve kabul edin</p>
      </div>

      {orders.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg border border-slate-200">
          <Package className="w-16 h-16 text-slate-300 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-slate-900 mb-2">Gelen Sipariş Yok</h3>
          <p className="text-slate-600">Şu anda yeni sipariş bulunmuyor</p>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <div key={order.id} className="bg-white rounded-lg border border-slate-200 p-6 hover:shadow-md transition-shadow">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
                <div>
                  <p className="text-sm text-slate-600 mb-1">Sipariş Numarası</p>
                  <p className="text-lg font-semibold text-slate-900">{order.trackingNumber}</p>
                </div>
                <div>
                  <p className="text-sm text-slate-600 mb-1">Ücret</p>
                  <p className="text-lg font-semibold text-blue-600">₺{order.price}</p>
                </div>
                <div>
                  <p className="text-sm text-slate-600 mb-1">Ağırlık</p>
                  <p className="text-lg font-semibold text-slate-900">{order.package.weight} kg</p>
                </div>
                <div>
                  <p className="text-sm text-slate-600 mb-1">Açıklama</p>
                  <p className="text-lg font-semibold text-slate-900">{order.package.description || '-'}</p>
                </div>
              </div>

              <div className="border-t border-slate-200 pt-4 mb-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                  <div>
                    <p className="text-sm text-slate-600 flex items-center gap-1 mb-1">
                      <MapPin className="w-4 h-4" />
                      Alım Adresi
                    </p>
                    <p className="text-slate-900 font-medium">{order.pickupAddress.street}</p>
                    <p className="text-sm text-slate-600">{order.pickupAddress.city}</p>
                  </div>
                  <div>
                    <p className="text-sm text-slate-600 flex items-center gap-1 mb-1">
                      <MapPin className="w-4 h-4" />
                      Teslimat Adresi
                    </p>
                    <p className="text-slate-900 font-medium">{order.deliveryAddress.street}</p>
                    <p className="text-sm text-slate-600">{order.deliveryAddress.city}</p>
                  </div>
                </div>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => handleAcceptOrder(order.id)}
                  className="flex-1 py-2 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors"
                >
                  Kabul Et
                </button>
                <button
                  onClick={() => handleRejectOrder(order.id)}
                  className="flex-1 py-2 bg-red-100 text-red-700 rounded-lg font-semibold hover:bg-red-200 transition-colors"
                >
                  Reddet
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

export default IncomingOrdersPage
