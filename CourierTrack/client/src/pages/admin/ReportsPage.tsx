import React, { useState, useEffect } from 'react'
import { adminAPI } from '@/services/api'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import { Download, TrendingUp } from 'lucide-react'
import { ToastType } from '@/components/common/Toast'

interface ReportsPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const ReportsPage: React.FC<ReportsPageProps> = ({ onAddToast }) => {
  const [reportType, setReportType] = useState('revenue')
  const [reportData, setReportData] = useState<any>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchReport()
  }, [reportType])

  const fetchReport = async () => {
    setLoading(true)
    try {
      let response;
      if (reportType === 'daily') {
        response = await adminAPI.getDailyReport()
      } else {
        response = await adminAPI.getRevenueReport()
      }
      
      if (response?.data?.isSuccess) {
        setReportData(response.data.data)
      } else {
        setReportData(response?.data)
      }
    } catch (err) {
      onAddToast?.('Rapor yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleExport = () => {
    let csv = '';
    if (reportType === 'revenue') {
      const dataArray = Array.isArray(reportData) ? reportData : [];
      csv = [
        ['Tarih', 'Toplam Sipariş', 'Gelir'],
        ...dataArray.map((item: any) => [
          item.date,
          item.totalOrders,
          item.revenue,
        ]),
      ]
        .map((row) => row.join(','))
        .join('\n')
    } else {
      csv = [
        ['Tarih', 'Toplam Sipariş', 'Teslim Edilen', 'İptal Edilen', 'Gelir'],
        [
          reportData?.date || '',
          reportData?.totalOrders || 0,
          reportData?.deliveredOrders || 0,
          reportData?.cancelledOrders || 0,
          reportData?.revenue || 0,
        ],
      ]
        .map((row) => row.join(','))
        .join('\n')
    }

    const element = document.createElement('a')
    element.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv))
    element.setAttribute('download', `${reportType}-report.csv`)
    element.style.display = 'none'
    document.body.appendChild(element)
    element.click()
    document.body.removeChild(element)
    onAddToast?.('Rapor indirildi', 'success')
  }

  return (
    <div className="space-y-8">
      <div className="flex justify-between items-start">
        <div>
          <h1 className="text-3xl font-bold text-slate-900 mb-2">Raporlar</h1>
          <p className="text-slate-600">Sistem raporlarını görüntüleyin ve indirin</p>
        </div>
        <button
          onClick={handleExport}
          className="px-6 py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors flex items-center gap-2"
        >
          <Download className="w-5 h-5" />
          İndir
        </button>
      </div>

      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-4">Rapor Türü</h2>
        <div className="flex gap-4 flex-wrap">
          {[
            { value: 'revenue', label: 'Gelir Raporu' },
            { value: 'daily', label: 'Günlük Rapor' },
          ].map((type) => (
            <button
              key={type.value}
              onClick={() => setReportType(type.value)}
              className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                reportType === type.value
                  ? 'bg-blue-600 text-white'
                  : 'bg-slate-200 text-slate-700 hover:bg-slate-300'
              }`}
            >
              {type.label}
            </button>
          ))}
        </div>
      </div>

      <div className="bg-white rounded-lg border border-slate-200 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-4 flex items-center gap-2">
          <TrendingUp className="w-5 h-5" />
          {reportType === 'revenue' ? 'Gelir Trendi' : 'Günlük Özet'}
        </h2>
        
        {reportType === 'revenue' && Array.isArray(reportData) ? (
          <ResponsiveContainer width="100%" height={400}>
            <BarChart data={reportData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="revenue" fill="#3B82F6" name="Gelir (₺)" />
              <Bar dataKey="totalOrders" fill="#10B981" name="Sipariş Sayısı" />
            </BarChart>
          </ResponsiveContainer>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 py-6">
            <div className="text-center">
              <p className="text-slate-600">Toplam Sipariş</p>
              <p className="text-2xl font-bold text-slate-900">{reportData?.totalOrders || 0}</p>
            </div>
            <div className="text-center">
              <p className="text-slate-600">Teslim Edilen</p>
              <p className="text-2xl font-bold text-green-600">{reportData?.deliveredOrders || 0}</p>
            </div>
            <div className="text-center">
              <p className="text-slate-600">İptal Edilen</p>
              <p className="text-2xl font-bold text-red-600">{reportData?.cancelledOrders || 0}</p>
            </div>
            <div className="text-center">
              <p className="text-slate-600">Gelir</p>
              <p className="text-2xl font-bold text-blue-600">₺{reportData?.revenue || 0}</p>
            </div>
          </div>
        )}
      </div>

      {reportType === 'revenue' && (
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg border border-slate-200 p-6 text-center">
          <p className="text-slate-600 text-sm mb-2">Toplam Gelir Seçilen Dönem</p>
          <p className="text-3xl font-bold text-blue-600">
            ₺{Array.isArray(reportData) ? reportData.reduce((acc, curr) => acc + (curr.revenue || 0), 0) : 0}
          </p>
        </div>
        <div className="bg-white rounded-lg border border-slate-200 p-6 text-center">
          <p className="text-slate-600 text-sm mb-2">Toplam Sipariş Seçilen Dönem</p>
          <p className="text-3xl font-bold text-green-600">
            {Array.isArray(reportData) ? reportData.reduce((acc, curr) => acc + (curr.totalOrders || 0), 0) : 0}
          </p>
        </div>
      </div>
      )}
    </div>
  )
}

export default ReportsPage
