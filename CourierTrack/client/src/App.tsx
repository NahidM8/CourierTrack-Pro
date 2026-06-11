import React, { useEffect, useState } from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from '@/context/AuthContext'
import Navbar from '@/components/common/Navbar'
import PrivateRoute from '@/components/common/PrivateRoute'
import Toast, { ToastType } from '@/components/common/Toast'
import { useAuth } from '@/context/AuthContext'
import { disconnectSignalR, initSignalRConnection } from '@/services/signalr'

// Public Pages
import HomePage from '@/pages/HomePage'
import LoginPage from '@/pages/LoginPage'
import RegisterPage from '@/pages/RegisterPage'
import TrackingPage from '@/pages/TrackingPage'
import UnauthorizedPage from '@/pages/UnauthorizedPage'

// Customer Pages
import CreateOrderPage from '@/pages/customer/CreateOrderPage'
import MyOrdersPage from '@/pages/customer/MyOrdersPage'
import LiveTrackingPage from '@/pages/customer/LiveTrackingPage'

// Courier Pages
import CourierDashboardPage from '@/pages/courier/DashboardPage'
import IncomingOrdersPage from '@/pages/courier/IncomingOrdersPage'
import ActiveDeliveryPage from '@/pages/courier/ActiveDeliveryPage'
import DeliveryHistoryPage from '@/pages/courier/DeliveryHistoryPage'

// Admin Pages
import AdminDashboardPage from '@/pages/admin/DashboardPage'
import CourierManagementPage from '@/pages/admin/CourierManagementPage'
import OrderManagementPage from '@/pages/admin/OrderManagementPage'
import LiveMapPage from '@/pages/admin/LiveMapPage'
import ReportsPage from '@/pages/admin/ReportsPage'
import UserManagementPage from '@/pages/admin/UserManagementPage'

interface ToastMessage {
  id: string
  message: string
  type: ToastType
}

let toastCounter = 0

const AppContent: React.FC = () => {
  const { isAuthenticated, isLoading } = useAuth()
  const [toasts, setToasts] = useState<ToastMessage[]>([])

  const addToast = (message: string, type: ToastType = 'info') => {
    const id = `${Date.now()}-${++toastCounter}`
    setToasts((prev) => [...prev, { id, message, type }])
  }

  const removeToast = (id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }

  useEffect(() => {
    if (isLoading) return

    if (isAuthenticated) {
      initSignalRConnection().catch((err) => {
        console.error('Failed to init SignalR:', err)
      })
      return
    }

    disconnectSignalR()
  }, [isAuthenticated, isLoading])

  return (
    <div className="min-h-screen bg-slate-50">
      <Navbar />
      
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Routes>
          {/* Public Routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/tracking/:trackingNumber" element={<TrackingPage />} />
          <Route path="/unauthorized" element={<UnauthorizedPage />} />

          {/* Customer Routes */}
          <Route
            path="/customer/create-order"
            element={
              <PrivateRoute requiredRoles={['CUSTOMER']}>
                <CreateOrderPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/customer/orders"
            element={
              <PrivateRoute requiredRoles={['CUSTOMER']}>
                <MyOrdersPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/customer/live-tracking/:orderId"
            element={
              <PrivateRoute requiredRoles={['CUSTOMER']}>
                <LiveTrackingPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />

          {/* Courier Routes */}
          <Route
            path="/courier/dashboard"
            element={
              <PrivateRoute requiredRoles={['COURIER']}>
                <CourierDashboardPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/courier/incoming-orders"
            element={
              <PrivateRoute requiredRoles={['COURIER']}>
                <IncomingOrdersPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/courier/active-delivery/:orderId"
            element={
              <PrivateRoute requiredRoles={['COURIER']}>
                <ActiveDeliveryPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/courier/delivery-history"
            element={
              <PrivateRoute requiredRoles={['COURIER']}>
                <DeliveryHistoryPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />

          {/* Admin Routes */}
          <Route
            path="/admin/dashboard"
            element={
              <PrivateRoute requiredRoles={['ADMIN']}>
                <AdminDashboardPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/users"
            element={
              <PrivateRoute requiredRoles={['ADMIN']}>
                <UserManagementPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/couriers"
            element={
              <PrivateRoute requiredRoles={['ADMIN']}>
                <CourierManagementPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/orders"
            element={
              <PrivateRoute requiredRoles={['ADMIN']}>
                <OrderManagementPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/live-map"
            element={
              <PrivateRoute requiredRoles={['ADMIN']}>
                <LiveMapPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/reports"
            element={
              <PrivateRoute requiredRoles={['ADMIN']}>
                <ReportsPage onAddToast={addToast} />
              </PrivateRoute>
            }
          />

          {/* Catch all */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>

      {/* Toast Container */}
      <div className="fixed bottom-4 right-4 z-50 space-y-2">
        {toasts.map((toast) => (
          <Toast
            key={toast.id}
            id={toast.id}
            message={toast.message}
            type={toast.type}
            onClose={removeToast}
          />
        ))}
      </div>
    </div>
  )
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </Router>
  )
}

export default App
