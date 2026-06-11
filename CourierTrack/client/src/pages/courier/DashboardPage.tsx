import React, { useState, useEffect } from 'react'
import { courierAPI } from '@/services/api'
import { useAuth } from '@/context/AuthContext'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line } from 'recharts'
import { Package, TrendingUp, DollarSign, Clock } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'

interface DashboardPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const DashboardPage: React.FC<DashboardPageProps> = ({ onAddToast }) => {
  const { user } = useAuth()
  const [stats, setStats] = useState({
    todayOrders: 0,
    totalDeliveries: 0,
    todayEarnings: 0,
    totalEarnings: 0,
    rating: 0,
  })
  const [chartData, setChartData] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchDashboardData()
  }, [])

  const fetchDashboardData = async () => {
    setLoading(true)
    try {
      if (!user?.id) {
        onAddToast?.('Kurye bilgisi bulunamadı', 'error')
        return
      }

      const profileRes = await courierAPI.getById(user.id)
      const earningsRes = await courierAPI.getEarnings('week')
      const courierData = profileRes.data?.data

      setStats({
        todayOrders: 0,
        totalDeliveries: courierData?.totalDeliveries || 0,
        todayEarnings: courierData?.earnings?.today || 0,
        totalEarnings: courierData?.earnings?.total || 0,
        rating: courierData?.rating || 0,
      })

      setChartData(earningsRes.data || [])
    } catch (err) {
      onAddToast?.('Veriler yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const StatCard = ({ icon: Icon, title, value, unit }: any) => (
    <div className="bg-white rounded-lg border border-slate-200 p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-slate-600 font-medium">{title}</h3>
        <Icon className="w-5 h-5 text-blue-600" />
      </div>
      <p className="text-3xl font-bold text-slate-900">
        {value}
        <span className="text-xl text-slate-500 ml-1">{unit}</span>
      </p>
    </div>
  )

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900 mb-2">Kurye Dashboard</h1>
          <p className="text-slate-600">BugÃ¼nÃ¼n Ã¶zeti ve istatistikler</p>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard icon={Package} title="Bugünkü Siparişler" value={stats.todayOrders} unit="adet" />
        <StatCard icon={TrendingUp} title="Toplam Teslimat" value={stats.totalDeliveries} unit="adet" />
        <StatCard icon={DollarSign} title="Bugün Kazanç" value={stats.todayEarnings} unit="₺" />
        <StatCard icon={Clock} title="Toplam Kazanç" value={stats.totalEarnings} unit="₺" />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h2 className="text-lg font-semibold text-slate-900 mb-4">Haftalık Kazanç</h2>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Line type="monotone" dataKey="earnings" stroke="#3B82F6" />
            </LineChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h2 className="text-lg font-semibold text-slate-900 mb-4">Teslimat Sayıları</h2>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="deliveries" fill="#10B981" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-4">Hızlı Eylemler</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button className="px-4 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors">
            Gelen Siparişleri Gör
          </button>
          <button className="px-4 py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors">
            Aktif Teslimatlar
          </button>
          <button className="px-4 py-3 bg-slate-100 text-slate-700 rounded-lg font-semibold hover:bg-slate-200 transition-colors">
            Durum Raporı
          </button>
        </div>
      </div>
    </div>
  )
}

export default DashboardPage
