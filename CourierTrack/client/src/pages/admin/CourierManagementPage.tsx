import React, { useState, useEffect } from 'react'
import { adminAPI } from '@/services/api'
import { Courier } from '@/types'
import { MapPin, ToggleRight, ToggleLeft } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'

interface CourierManagementPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const CourierManagementPage: React.FC<CourierManagementPageProps> = ({ onAddToast }) => {
  const [couriers, setCouriers] = useState<Courier[]>([])
  const [loading, setLoading] = useState(true)
  const [statusFilter, setStatusFilter] = useState('all')

  useEffect(() => {
    fetchCouriers()
  }, [statusFilter])

  const fetchCouriers = async () => {
    setLoading(true)
    try {
      const response = await adminAPI.getCouriers(statusFilter === 'all' ? undefined : statusFilter)
      setCouriers(response.data)
    } catch (err) {
      onAddToast?.('Kuryeler yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleToggleCourier = async (courierId: string) => {
    try {
      await adminAPI.toggleCourierStatus(courierId)
      onAddToast?.('Durum güncellendi', 'success')
      fetchCouriers()
    } catch (err) {
      onAddToast?.('İşlem başarısız', 'error')
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900 mb-2">Kurye YÃ¶netimi</h1>
          <p className="text-slate-600">TÃ¼m kuryeleri yÃ¶netin ve izleyin</p>
        </div>
      </div>

      <div className="flex gap-2 mb-6 flex-wrap">
        {['all', 'active', 'inactive'].map((status) => (
          <button
            key={status}
            onClick={() => setStatusFilter(status)}
            className={`px-4 py-2 rounded-lg font-medium transition-colors ${
              statusFilter === status
                ? 'bg-blue-600 text-white'
                : 'bg-slate-200 text-slate-700 hover:bg-slate-300'
            }`}
          >
            {status === 'all' ? 'Tümü' : status === 'active' ? 'Aktif' : 'Pasif'}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">İsim</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Araç</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Konum</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Teslimatlar</th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-slate-900">Rating</th>
                <th className="px-6 py-4 text-center text-sm font-semibold text-slate-900">Durum</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {couriers.map((courier) => (
                <tr key={courier.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-6 py-4">
                    <p className="font-semibold text-slate-900">Kurye İsmi</p>
                  </td>
                  <td className="px-6 py-4">
                    <p className="text-slate-900">{courier.vehicle.type}</p>
                  </td>
                  <td className="px-6 py-4">
                    <button className="text-blue-600 hover:text-blue-700 flex items-center gap-1">
                      <MapPin className="w-4 h-4" />
                      Haritada Gör
                    </button>
                  </td>
                  <td className="px-6 py-4 text-slate-900">{courier.totalDeliveries}</td>
                  <td className="px-6 py-4">
                    <span className="text-yellow-600">★ {courier.rating.toFixed(1)}</span>
                  </td>
                  <td className="px-6 py-4 text-center">
                    <button
                      onClick={() => handleToggleCourier(courier.id)}
                      className="text-slate-600 hover:text-slate-900"
                    >
                      {courier.isAvailable ? (
                        <ToggleRight className="w-6 h-6 text-green-600" />
                      ) : (
                        <ToggleLeft className="w-6 h-6 text-red-600" />
                      )}
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

export default CourierManagementPage
