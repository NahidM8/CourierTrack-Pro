import React from 'react'
import { Link } from 'react-router-dom'
import { Truck, Clock, MapPin, Shield } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'

const HomePage: React.FC = () => {
  const { isAuthenticated, user } = useAuth()

  return (
    <div>
      {/* Hero Section */}
      <section className="bg-gradient-to-br from-blue-50 via-slate-50 to-slate-50 py-20 sm:py-24 lg:py-32">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold text-slate-900 mb-6 leading-tight">
            Kargo Takibi Hiç Bu Kadar Kolay Olmadı
          </h1>
          <p className="text-lg sm:text-xl text-slate-600 mb-10 max-w-3xl mx-auto leading-relaxed">
            CourierTrack Pro ile, paketlerinizin her anını takip edin. Güvenilir, hızlı ve şeffaf hizmet.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            {!isAuthenticated ? (
              <>
                <Link
                  to="/register"
                  className="px-6 py-3 h-12 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2"
                >
                  Hemen Başla
                </Link>
                <Link
                  to="/login"
                  className="px-6 py-3 h-12 border-2 border-blue-600 text-blue-600 rounded-lg font-semibold hover:bg-blue-50 transition-colors flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2"
                >
                  Giriş Yap
                </Link>
              </>
            ) : user?.role === 'CUSTOMER' ? (
              <>
                <Link
                  to="/customer/create-order"
                  className="px-6 py-3 h-12 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2"
                >
                  Sipariş Oluştur
                </Link>
                <Link
                  to="/customer/orders"
                  className="px-6 py-3 h-12 border-2 border-blue-600 text-blue-600 rounded-lg font-semibold hover:bg-blue-50 transition-colors flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2"
                >
                  Siparişlerim
                </Link>
              </>
            ) : user?.role === 'ADMIN' ? (
              <Link
                to="/admin/users"
                className="px-6 py-3 h-12 bg-purple-600 text-white rounded-lg font-semibold hover:bg-purple-700 transition-colors flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-purple-600 focus:ring-offset-2"
              >
                Yönetici Paneli (Kullanıcılar)
              </Link>
            ) : (
              <Link
                to="/courier/dashboard"
                className="px-6 py-3 h-12 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2"
              >
                Kurye Paneli
              </Link>
            )}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-16 sm:py-20 lg:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div className="bg-white p-6 rounded-lg border border-slate-200 h-full flex flex-col hover:shadow-lg transition-shadow">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-semibold text-lg text-slate-900">Hızlı Teslimat</h3>
                <Truck className="w-6 h-6 text-blue-600 flex-shrink-0" />
              </div>
              <p className="text-slate-600 flex-grow">Güvenilir kuriye ağımız ile en kısa sürede teslimat.</p>
            </div>

            <div className="bg-white p-6 rounded-lg border border-slate-200 h-full flex flex-col hover:shadow-lg transition-shadow">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-semibold text-lg text-slate-900">Canlı Takip</h3>
                <MapPin className="w-6 h-6 text-green-600 flex-shrink-0" />
              </div>
              <p className="text-slate-600 flex-grow">Paketinizin konumunu gerçek zamanlı izleyin.</p>
            </div>

            <div className="bg-white p-6 rounded-lg border border-slate-200 h-full flex flex-col hover:shadow-lg transition-shadow">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-semibold text-lg text-slate-900">24/7 Hizmet</h3>
                <Clock className="w-6 h-6 text-amber-600 flex-shrink-0" />
              </div>
              <p className="text-slate-600 flex-grow">Gece gündüz, her an biz yanınızdayız.</p>
            </div>

            <div className="bg-white p-6 rounded-lg border border-slate-200 h-full flex flex-col hover:shadow-lg transition-shadow">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-semibold text-lg text-slate-900">Güvenli</h3>
                <Shield className="w-6 h-6 text-red-600 flex-shrink-0" />
              </div>
              <p className="text-slate-600 flex-grow">Paketiniz tamamen sigortalı ve korumalıdır.</p>
            </div>
          </div>
        </div>
      </section>

      {/* Quick Tracking */}
      {!isAuthenticated && (
        <section className="py-16 sm:py-20 lg:py-24">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="bg-white rounded-lg border border-slate-200 p-8 sm:p-10">
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 mb-8 text-center">Takip Numaranız Var mı?</h2>
              <div className="max-w-md mx-auto flex flex-col sm:flex-row gap-2">
                <input
                  type="text"
                  placeholder="Takip numarasını giriniz..."
                  className="flex-1 w-full px-4 py-3 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-transparent transition-colors"
                />
                <button className="px-6 py-3 h-12 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors whitespace-nowrap focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2">
                  Takip Et
                </button>
              </div>
            </div>
          </div>
        </section>
      )}

      {/* Pricing */}
      <section className="bg-slate-50 py-16 sm:py-20 lg:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl sm:text-4xl font-bold text-slate-900 mb-4">Basit ve Şeffaf Fiyatlandırma</h2>
            <p className="text-lg text-slate-600 max-w-2xl mx-auto">Uzaklık ve ağırlığa göre adil fiyatlandırma</p>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[
              { weight: 'Işık (0-1kg)', price: '₺25' },
              { weight: 'Normal (1-5kg)', price: '₺35' },
              { weight: 'Ağır (5kg+)', price: '₺50' },
            ].map((plan) => (
              <div key={plan.weight} className="bg-white p-8 rounded-lg border border-slate-200 text-center hover:shadow-lg transition-shadow h-full flex flex-col justify-center">
                <h3 className="text-lg font-semibold text-slate-900 mb-4">{plan.weight}</h3>
                <p className="text-4xl font-bold text-blue-600 mb-4">{plan.price}</p>
                <p className="text-slate-600 text-sm">Başlangıç fiyatı - Uzaklığa göre değişir</p>
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  )
}

export default HomePage
