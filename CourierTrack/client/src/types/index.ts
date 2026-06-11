export type UserRole = 'CUSTOMER' | 'COURIER' | 'ADMIN'

export interface User {
  id: string
  email: string
  name: string
  phone: string
  role: UserRole
  profileImage?: string
  isActive: boolean
  createdAt: string
}

export interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
}

export interface Address {
  id: string
  street: string
  city: string
  state: string
  zipCode: string
  country: string
  latitude: number
  longitude: number
  label?: string
}

export interface Package {
  weight: number
  dimensions: {
    length: number
    width: number
    height: number
  }
  description: string
  fragile: boolean
  value: number
}

export interface Order {
  id: string
  customerId: string
  courierId?: string
  pickupAddress: Address
  deliveryAddress: Address
  package: Package
  status: OrderStatus
  price: number
  estimatedDelivery?: string
  actualDelivery?: string
  trackingNumber: string
  paymentStatus: PaymentStatus
  notes?: string
  createdAt: string
  updatedAt: string
}

export type OrderStatus =
  | 'CREATED'
  | 'PENDING'
  | 'ASSIGNED'
  | 'PICKED_UP'
  | 'IN_TRANSIT'
  | 'DELIVERED'
  | 'FAILED'
  | 'CANCELLED'
export type PaymentStatus = 'PENDING' | 'COMPLETED' | 'FAILED' | 'REFUNDED'

export interface Courier {
  id: string
  userId: string
  vehicle: {
    type: 'BIKE' | 'CAR' | 'VAN'
    licensePlate: string
  }
  isAvailable: boolean
  currentLocation?: {
    latitude: number
    longitude: number
    updatedAt: string
  }
  totalDeliveries: number
  rating: number
  earnings: {
    today: number
    total: number
  }
}

export interface DeliveryStatus {
  orderId: string
  courierId: string
  currentLocation: {
    latitude: number
    longitude: number
  }
  status: OrderStatus
  estimatedArrival: string
}

export interface Notification {
  id: string
  userId: string
  type: 'ORDER_CREATED' | 'ORDER_ASSIGNED' | 'PICKUP_CONFIRMED' | 'DELIVERY_STARTED' | 'DELIVERY_COMPLETED' | 'PAYMENT_RECEIVED'
  title: string
  message: string
  data?: Record<string, any>
  read: boolean
  createdAt: string
}

export interface RouteInfo {
  distance: string
  duration: string
  polyline: string
}
