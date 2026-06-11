import React, { useState, useEffect } from 'react'
import { adminAPI } from '@/services/api'
import MapComponent from '@/components/common/MapComponent'
import { initSignalRConnection, joinAdminGroup, leaveAdminGroup, onLocationUpdate } from '@/services/signalr'
import { Truck } from 'lucide-react'

interface LiveMapPageProps {
  onAddToast?: (message: string, type: string) => void
}

const LiveMapPage: React.FC<LiveMapPageProps> = ({ onAddToast }) => {
  const [couriers, setCouriers] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchCouriers()

    const setupRealtime = async () => {
      await initSignalRConnection()
      await joinAdminGroup()
    }

    setupRealtime().catch((err) => {
      console.error('Failed to join admin tracking group:', err)
    })

    onLocationUpdate((data) => {
      const courierId = data.courierId?.toString?.() ?? data.courierId
      const location = data.currentLocation ?? {
        latitude: data.latitude,
        longitude: data.longitude,
      }

      setCouriers((prev) =>
        prev.map((c) =>
          c.id === courierId
            ? { ...c, location }
            : c
        )
      )
    })

    return () => {
      leaveAdminGroup().catch(() => undefined)
    }
  }, [])

  const fetchCouriers = async () => {
    try {
      const response = await adminAPI.getCouriers('active')
      setCouriers(response.data.map((c: any) => ({
        id: c.id,
        name: 'Kurye Adı',
        location: c.currentLocation || { latitude: 41.0082, longitude: 28.9784 },
      })))
    } catch (err) {
      onAddToast?.('Kuryeler yüklenemedi', 'error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900 mb-2">Canlı Harita</h1>
        <p className="text-slate-600">Tüm aktif kuryelerin konumunu gerçek zamanlı izleyin</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        <div className="lg:col-span-3 bg-white rounded-lg border border-slate-200 overflow-hidden">
          <MapComponent
            center={{ lat: 41.0082, lng: 28.9784 }}
            zoom={12}
            markers={couriers.map((c) => ({
              position: {
                lat: c.location.latitude,
                lng: c.location.longitude,
              },
              title: c.name,
              color: 'blue',
              infoContent: `${c.name}: ${c.location.latitude.toFixed(4)}, ${c.location.longitude.toFixed(4)}`,
            }))}
            interactive={false}
          />
        </div>

        <div className="bg-white rounded-lg border border-slate-200 p-6">
          <h2 className="text-lg font-semibold text-slate-900 mb-4 flex items-center gap-2">
            <Truck className="w-5 h-5" />
            Aktif Kuryeler
          </h2>
          <div className="space-y-3 max-h-96 overflow-y-auto">
            {couriers.map((courier) => (
              <div
                key={courier.id}
                className="p-3 bg-slate-50 rounded-lg border border-slate-200 hover:border-blue-400 transition-colors cursor-pointer"
              >
                <p className="font-semibold text-slate-900">{courier.name}</p>
                <p className="text-sm text-slate-600">
                  {courier.location.latitude.toFixed(4)}, {courier.location.longitude.toFixed(4)}
                </p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}

export default LiveMapPage
