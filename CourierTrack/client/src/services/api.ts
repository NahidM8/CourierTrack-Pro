import axios, { AxiosInstance, AxiosError } from 'axios'
import Cookie from 'js-cookie'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5000'

const client: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Add token to requests
client.interceptors.request.use((config) => {
  const token = Cookie.get('authToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Handle errors
client.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      // Token expired, clear auth
      Cookie.remove('authToken')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export const apiClient = client

// Auth endpoints
export const authAPI = {
  register: (data: {
    fullName: string
    email: string
    password: string
    phoneNumber: string
    role: number
    vehicleType: number
  }) => client.post('/api/v1/auth/register', data),
  login: (email: string, password: string) =>
    client.post('/api/v1/auth/login', { email, password }),
  logout: () => client.post('/api/auth/logout'),
  verify: () => client.get('/api/auth/verify'),
}

// Orders endpoints
export const ordersAPI = {
  create: (data: any) => client.post('/api/orders', data),
  getById: (id: string) => client.get(`/api/orders/${id}`),
  getMyOrders: (filter?: string) => client.get('/api/orders/my', { params: { status: filter } }),
  getAll: (filter?: string) => client.get('/api/orders', { params: { status: filter } }),
  updateStatus: (id: string, data: {
    courierId: string
    status: number
    pickedUpAt: string | null
    deliveredAt: string | null
    note: string
  }) => client.put(`/api/v1/orders/${id}/status`, data),
  assignCourier: (id: string, courierId: string) =>
    client.patch(`/api/orders/${id}/assign`, { courierId }),
  calculatePrice: (data: any) => client.post('/api/orders/calculate-price', data),
}

// Courier endpoints
export const courierAPI = {
  getById: (courierId: string) => client.get(`/api/Couriers/${courierId}`),
  updateAvailability: (courierId: string, isAvailable: boolean) =>
    client.put(`/api/Couriers/${courierId}/availability`, { isAvailable }),
  updateLocation: (courierId: string, latitude: number, longitude: number) =>
    client.put(`/api/Couriers/${courierId}/location`, { latitude, longitude }),
  getOrders: (courierId: string) => client.get(`/api/Couriers/${courierId}/orders`),
  getDeliveryHistory: () => client.get('/api/courier/deliveries/history'),
  getEarnings: (period?: string) =>
    client.get('/api/courier/earnings', { params: { period } }),
}

// Admin endpoints
export const adminAPI = {
  getUsers: () => client.get('/api/v1/admin/users'),
  getUserById: (id: string) => client.get(`/api/v1/admin/users/${id}`),
  updateUserStatus: (id: string, isActive: boolean) =>
    client.put(`/api/v1/admin/users/${id}/status`, { isActive }),
  getDashboard: () => client.get('/api/v1/admin/dashboard'),
  getDailyReport: () => client.get('/api/v1/admin/reports/daily'),
  getRevenueReport: () => client.get('/api/v1/admin/reports/revenue'),
  getCouriers: (status?: string) =>
    client.get('/api/v1/admin/couriers', { params: { status } }),
  toggleCourierStatus: (courierId: string) =>
    client.patch(`/api/v1/admin/couriers/${courierId}/toggle`),
  getAllOrders: (filter?: string) =>
    client.get('/api/v1/admin/orders', { params: { status: filter } }),
  getReports: (type: string) =>
    client.get('/api/v1/admin/reports', { params: { type } }),
}

// Payment endpoints
export const paymentAPI = {
  createPaymentIntent: (data: { orderId: string; amount: number }) =>
    client.post('/api/payments/create-intent', data),
  createCheckout: (orderId: string) =>
    client.post('/api/payments/checkout', { orderId }),
  confirmPayment: (paymentIntentId: string) =>
    client.post('/api/payments/confirm', { paymentIntentId }),
}

// Tracking endpoints
export const trackingAPI = {
  trackByNumber: (trackingNumber: string) =>
    client.get(`/api/tracking/${trackingNumber}`),
}
