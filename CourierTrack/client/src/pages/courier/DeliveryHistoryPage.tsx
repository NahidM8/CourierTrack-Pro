import React, { useState, useEffect } from 'react'
import { courierAPI } from '@/services/api'
import { Order } from '@/types'
import { CheckCircle, TrendingUp } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'
import { useAuth } from '@/context/AuthContext'

interface DeliveryHistoryPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const DeliveryHistoryPage: React.FC<DeliveryHistoryPageProps> = ({ onAddToast }) => {
  const { user } = useAuth()
  const [deliveries, setDeliveries] = useState<Order[]>([])
  const [earnings, setEarnings] = useState({ today: 0, week: 0, month: 0, total: 0 })
  const [loading, setLoading] = useState(true)
  const [selectedPeriod, setSelectedPeriod] = useState('all')

  useEffect(() => {
    fetchDeliveryHistory()
  }, [])

  const fetchDeliveryHistory = async () => {
    setLoading(true)
    try {
      if (!user?.id) {
        onAddToast?.('Kurye bilgisi bulunamadı', 'error')
        return
      }

      const historyRes = await courierAPI.getOrders(user.id)
      const earningsRes = await courierAPI.getEarnings()
      setDeliveries(historyRes.data?.data || [])
      setEarnings(earningsRes.data)
    } catch (err) {
      onAddToast?.('Geçmiş yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const getEarningsForPeriod = () => {
    switch (selectedPeriod) {
      case 'today':
        return earnings.today
      case 'week':
        return earnings.week
      case 'month':
        return earnings.month
      default:
        return earnings.total
    }
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Geçmiş Teslimatlar</h1>
        <p className="text-slate-600">Tamamlanan teslimatları ve kazançlarınızı görüntüleyin</p>
      </div>

      {/* Earnings Summary */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="bg-white rounded-lg border border-slate-200 p-6 cursor-pointer hover:shadow-md transition-shadow"
          onClick={() => setSelectedPeriod('today')}>
          <p className="text-slate-600 text-sm mb-2">Bugün</p>
          <p className="text-2xl font-bold text-slate-900">₺{earnings.today}</p>
        </div>
        <div className="bg-white rounded-lg border border-slate-200 p-6 cursor-pointer hover:shadow-md transition-shadow"
          onClick={() => setSelectedPeriod('week')}>
          <p className="text-slate-600 text-sm mb-2">Bu Hafta</p>
          <p className="text-2xl font-bold text-slate-900">₺{earnings.week}</p>
        </div>
        <div className="bg-white rounded-lg border border-slate-200 p-6 cursor-pointer hover:shadow-md transition-shadow"
          onClick={() => setSelectedPeriod('month')}>
          <p className="text-slate-600 text-sm mb-2">Bu Ay</p>
          <p className="text-2xl font-bold text-slate-900">₺{earnings.month}</p>
        </div>
        <div className="bg-white rounded-lg border border-slate-200 p-6 bg-blue-50"
          onClick={() => setSelectedPeriod('all')}>
          <p className="text-blue-600 text-sm mb-2 flex items-center gap-1">
            <TrendingUp className="w-4 h-4" />
            Toplam
          </p>
          <p className="text-2xl font-bold text-blue-600">₺{earnings.total}</p>
        </div>
      </div>

      {/* Deliveries List */}
      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Sipariş</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Müşteri Adresi</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Tarih</th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-slate-900">Kazanç</th>
                <th className="px-6 py-4 text-center text-sm font-semibold text-slate-900">Durum</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {deliveries.map((delivery) => (
                <tr key={delivery.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-6 py-4">
                    <p className="font-semibold text-slate-900">{delivery.trackingNumber}</p>
                  </td>
                  <td className="px-6 py-4">
                    <p className="text-slate-900">{delivery.deliveryAddress.street}</p>
                    <p className="text-sm text-slate-600">{delivery.deliveryAddress.city}</p>
                  </td>
                  <td className="px-6 py-4 text-slate-600">
                    {new Date(delivery.updatedAt).toLocaleDateString('tr-TR')}
                  </td>
                  <td className="px-6 py-4 text-right font-semibold text-green-600">
                    ₺{delivery.price}
                  </td>
                  <td className="px-6 py-4 text-center">
                    <span className="inline-flex items-center gap-1 px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm font-medium">
                      <CheckCircle className="w-4 h-4" />
                      Teslim
                    </span>
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

export default DeliveryHistoryPage
