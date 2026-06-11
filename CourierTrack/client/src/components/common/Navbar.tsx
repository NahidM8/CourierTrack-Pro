import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Menu, X, LogOut, User } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'

const Navbar: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)

  const handleLogout = async () => {
    await logout()
    navigate('/login')
    setIsMobileMenuOpen(false)
  }

  const getMenuItems = () => {
    if (!isAuthenticated || !user) return []

    const baseItems = [
      { label: 'Ana Sayfa', href: '/' },
    ]

    switch (user.role) {
      case 'CUSTOMER':
        return [
          ...baseItems,
          { label: 'Sipariş Oluştur', href: '/customer/create-order' },
          { label: 'Siparişlerim', href: '/customer/orders' },
          { label: 'Canlı Takip', href: '/customer/live-tracking' },
        ]
      case 'COURIER':
        return [
          ...baseItems,
          { label: 'Dashboard', href: '/courier/dashboard' },
          { label: 'Gelen Siparişler', href: '/courier/incoming-orders' },
          { label: 'Aktif Teslimat', href: '/courier/active-delivery' },
          { label: 'Geçmiş', href: '/courier/delivery-history' },
        ]
      case 'ADMIN':
        return [
          ...baseItems,
          { label: 'Dashboard', href: '/admin/dashboard' },
          { label: 'Kurye Yönetimi', href: '/admin/couriers' },
          { label: 'Sipariş Yönetimi', href: '/admin/orders' },
          { label: 'Canlı Harita', href: '/admin/live-map' },
          { label: 'Raporlar', href: '/admin/reports' },
        ]
      default:
        return baseItems
    }
  }

  const menuItems = getMenuItems()

  return (
    <nav className="bg-white shadow-sm border-b border-slate-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2">
            <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
              <span className="text-white font-bold text-xl">C</span>
            </div>
            <span className="font-bold text-lg text-slate-900">CourierTrack</span>
          </Link>

          {/* Desktop Menu */}
          <div className="hidden md:flex items-center gap-8">
            {menuItems.map((item) => (
              <Link
                key={item.href}
                to={item.href}
                className="text-slate-600 hover:text-blue-600 transition-colors text-sm font-medium"
              >
                {item.label}
              </Link>
            ))}
          </div>

          {/* Auth Actions */}
          <div className="hidden md:flex items-center gap-4">
            {isAuthenticated && user ? (
              <div className="flex items-center gap-4">
                <span className="text-sm text-slate-600">{user.name}</span>
                <button
                  onClick={handleLogout}
                  className="flex items-center gap-2 px-4 py-2 bg-slate-100 text-slate-700 rounded-lg hover:bg-slate-200 transition-colors focus:outline-none focus:ring-2 focus:ring-slate-400 focus:ring-offset-2"
                >
                  <LogOut className="w-4 h-4" />
                  Çıkış
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2">
                <Link
                  to="/login"
                  className="px-4 py-2 text-slate-600 hover:text-blue-600 transition-colors focus:outline-none rounded-lg focus:ring-2 focus:ring-blue-600"
                >
                  Giriş
                </Link>
                <Link
                  to="/register"
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2"
                >
                  Kaydol
                </Link>
              </div>
            )}
          </div>

          {/* Mobile Menu Button */}
          <button
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            className="md:hidden p-2 hover:bg-slate-100 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-600 transition-colors"
          >
            {isMobileMenuOpen ? (
              <X className="w-6 h-6" />
            ) : (
              <Menu className="w-6 h-6" />
            )}
          </button>
        </div>

        {/* Mobile Menu */}
        {isMobileMenuOpen && (
          <div className="md:hidden border-t border-slate-200 py-4 space-y-2">
            {menuItems.map((item) => (
              <Link
                key={item.href}
                to={item.href}
                className="block px-4 py-2 text-slate-600 hover:bg-slate-100 rounded-lg transition-colors"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                {item.label}
              </Link>
            ))}
            {isAuthenticated && user && (
              <button
                onClick={handleLogout}
                className="w-full px-4 py-2 text-left text-slate-600 hover:bg-slate-100 rounded-lg transition-colors flex items-center gap-2"
              >
                <LogOut className="w-4 h-4" />
                Çıkış
              </button>
            )}
            {!isAuthenticated && (
              <>
                <Link
                  to="/login"
                  className="block px-4 py-2 text-slate-600 hover:bg-slate-100 rounded-lg"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  Giriş
                </Link>
                <Link
                  to="/register"
                  className="block px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  Kaydol
                </Link>
              </>
            )}
          </div>
        )}
      </div>
    </nav>
  )
}

export default Navbar
