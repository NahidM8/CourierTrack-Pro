import React, { useState, useEffect } from 'react'
import { adminAPI } from '@/services/api'
import { BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import { Truck, Package, DollarSign, TrendingUp } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'

interface AdminDashboardPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const DashboardPage: React.FC<AdminDashboardPageProps> = ({ onAddToast }) => {
  const [dashboard, setDashboard] = useState({
    totalOrders: 0,
    activeCouriers: 0,
    totalUsers: 0,
    totalRevenue: 0,
    pendingOrders: 0,
    deliveredOrders: 0,
  })
  const [revenueData, setRevenueData] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    setLoading(true)
    try {
      const [dashRes, revRes] = await Promise.all([
        adminAPI.getDashboard(),
        adminAPI.getRevenueReport()
      ])
      
      if (dashRes.data.isSuccess) setDashboard(dashRes.data.data)
      else setDashboard(dashRes.data)

      if (revRes.data.isSuccess) setRevenueData(revRes.data.data)
      else setRevenueData(revRes.data)
    } catch (err) {
      onAddToast?.('Veriler yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const StatCard = ({ icon: Icon, title, value, unit, color }: any) => (
    <div className="bg-white rounded-lg border border-slate-200 p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-slate-600 font-medium">{title}</h3>
        <div className={`p-3 rounded-lg ${color}`}>
          <Icon className="w-5 h-5 text-white" />
        </div>
      </div>
      <p className="text-3xl font-bold text-slate-900">
        {value}
        <span className="text-xl text-slate-500 ml-1">{unit}</span>
      </p>
    </div>
  )

  const COLORS = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444']

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900 mb-2">Admin Dashboard</h1>
          <p className="text-slate-600">Sistem Ã¶zeti ve istatistikler</p>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          icon={Package}
          title="Toplam Siparişler"
          value={dashboard.totalOrders}
          unit="adet"
          color="bg-blue-600"
        />
        <StatCard
          icon={Truck}
          title="Aktif Kuryeler"
          value={dashboard.activeCouriers}
          unit="kişi"
          color="bg-green-600"
        />
        <StatCard
          icon={DollarSign}
          title="Toplam Gelir"
          value={dashboard.totalRevenue}
          unit="₺"
          color="bg-amber-600"
        />
        <StatCard
          icon={TrendingUp}
          title="Beklemede"
          value={dashboard.pendingOrders}
          unit="adet"
          color="bg-red-600"
        />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h2 className="text-lg font-semibold text-slate-900 mb-4">Haftalık Sipariş Trendi</h2>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={revenueData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Line type="monotone" dataKey="totalOrders" stroke="#3B82F6" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* Daily Revenue Chart */}
        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h2 className="text-lg font-semibold text-slate-900 mb-4">Günlük Gelir</h2>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={revenueData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="revenue" fill="#3B82F6" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-4">Hızlı Eylemler</h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <button onClick={() => window.location.href='/admin/couriers'} className="px-4 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors">
            Kurye Yönetimi
          </button>
          <button onClick={() => window.location.href='/admin/orders'} className="px-4 py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors">
            Sipariş Yönetimi
          </button>
          <button onClick={() => window.location.href='/admin/users'} className="px-4 py-3 bg-purple-600 text-white rounded-lg font-semibold hover:bg-purple-700 transition-colors">
            Kullanıcı Yönetimi
          </button>
          <button onClick={() => window.location.href='/admin/reports'} className="px-4 py-3 bg-slate-100 text-slate-700 rounded-lg font-semibold hover:bg-slate-200 transition-colors">
            Raporlar
          </button>
        </div>
      </div>
    </div>
  )
}

export default DashboardPage
