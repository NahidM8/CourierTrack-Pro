# CourierTrack Pro - Hızlı Referans

Sık kullanılan kod örnekleri ve pattern'lar.

## Kurulum (30 saniye)

```bash
pnpm install
pnpm dev
# Açılır: http://localhost:3000
```

## Environment Setup

```bash
# .env dosyasını oluştur
VITE_API_BASE_URL=https://localhost:5000
VITE_SIGNALR_HUB_URL=https://localhost:5000/hubs/tracking
VITE_GOOGLE_MAPS_API_KEY=YOUR_KEY
VITE_STRIPE_PUBLIC_KEY=YOUR_KEY
```

## Sık Kullanılan Imports

```typescript
// Auth
import { useAuth } from '@/context/AuthContext'

// API Calls
import { ordersAPI, courierAPI, adminAPI } from '@/services/api'

// Types
import { Order, Courier, User, OrderStatus } from '@/types'

// Components
import LoadingSpinner from '@/components/common/LoadingSpinner'
import PrivateRoute from '@/components/common/PrivateRoute'
import MapComponent from '@/components/common/MapComponent'

// SignalR
import { 
  onLocationUpdate, 
  onOrderStatusChange, 
  sendLocationUpdate 
} from '@/services/signalr'

// UI
import { CheckCircle, MapPin, Package } from 'lucide-react'
```

## Auth Kullanımı

```typescript
// Komponentin içinde auth state'e erişim
const { user, isAuthenticated, login, logout } = useAuth()

// Protected route oluştur
<PrivateRoute requiredRoles={['CUSTOMER']}>
  <CustomerPage />
</PrivateRoute>

// Admin-only route
<PrivateRoute requiredRoles={['ADMIN']}>
  <AdminDashboard />
</PrivateRoute>
```

## API Çağrıları

```typescript
// Sipariş oluştur
const response = await ordersAPI.create({
  pickupAddress: {...},
  deliveryAddress: {...},
  package: {...},
  price: 100
})

// Kurye profilini al
const courier = await courierAPI.getProfile()

// Admin dashboard verisi
const dashboard = await adminAPI.getDashboard()

// Hata yönetimi
try {
  await ordersAPI.getMyOrders()
} catch (err) {
  console.error(err)
  onAddToast('Hata oluştu', 'error')
}
```

## State Management

```typescript
// Context API ile auth
const { user, token } = useAuth()

// Local state
const [orders, setOrders] = useState<Order[]>([])
const [loading, setLoading] = useState(false)

// useEffect ile veri getir
useEffect(() => {
  const fetch = async () => {
    setLoading(true)
    try {
      const res = await ordersAPI.getMyOrders()
      setOrders(res.data)
    } finally {
      setLoading(false)
    }
  }
  fetch()
}, [])
```

## Real-time Features

```typescript
// Konum güncellemelerini dinle
onLocationUpdate((data) => {
  console.log('Kurye konumu:', data.currentLocation)
  setDeliveryStatus(data)
})

// Sipariş durum değişikliklerini dinle
onOrderStatusChange((data) => {
  console.log('Sipariş durumu:', data.status)
  setOrder(prev => ({ ...prev, status: data.status }))
})

// Konum gönder
await sendLocationUpdate(latitude, longitude)

// Sipariş durumu güncelle
await sendOrderStatusUpdate(orderId, 'DELIVERED')
```

## Harita Kullanımı

```typescript
// Basit harita göster
<MapComponent
  center={{ lat: 41.0082, lng: 28.9784 }}
  zoom={13}
  height="400px"
/>

// Markerlar ekle
<MapComponent
  markers={[
    { position: { lat: 41.0, lng: 28.9 }, title: 'Alım', color: 'green' },
    { position: { lat: 41.1, lng: 28.8 }, title: 'Teslimat', color: 'red' }
  ]}
/>

// Adres seçimi
<MapComponent
  interactive={true}
  onAddressSelect={(address) => {
    setPickupAddress(address)
  }}
/>
```

## Form Yönetimi

```typescript
// Form state
const [form, setForm] = useState({
  email: '',
  password: '',
})

// Input handler
const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
  const { name, value } = e.target
  setForm(prev => ({ ...prev, [name]: value }))
}

// Submit
const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault()
  try {
    await authAPI.login(form.email, form.password)
  } catch (err) {
    setError('Hata oluştu')
  }
}

// Form JSX
<input
  type="email"
  name="email"
  value={form.email}
  onChange={handleChange}
  className="w-full px-4 py-2 border border-slate-300 rounded-lg"
/>
```

## Conditional Rendering

```typescript
// Loading state
{loading ? (
  <LoadingSpinner message="Yükleniyor..." />
) : orders.length === 0 ? (
  <div>Sipariş bulunamadı</div>
) : (
  <OrderList orders={orders} />
)}

// Status badge
<span className={getStatusColor(order.status)}>
  {order.status}
</span>

// Rol-based UI
{user?.role === 'ADMIN' && (
  <AdminPanel />
)}
```

## Toast Notifications

```typescript
// Toast ekle (ToastType: 'success' | 'error' | 'info' | 'warning')
onAddToast('Sipariş oluşturuldu', 'success')
onAddToast('Hata oluştu', 'error')
onAddToast('Bilgi', 'info')

// Async işlem sonrası toast
try {
  await api.create(data)
  onAddToast('Başarılı', 'success')
} catch {
  onAddToast('Başarısız', 'error')
}
```

## Typing Örnekleri

```typescript
// Component props
interface CreateOrderPageProps {
  onAddToast: (message: string, type: ToastType) => void
}

// API response
interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}

// Form data
interface OrderForm {
  pickupAddress: Address
  deliveryAddress: Address
  package: Package
}
```

## Styling Patterns

```typescript
// Conditional className
<button className={`px-4 py-2 rounded-lg ${
  isActive ? 'bg-blue-600 text-white' : 'bg-slate-100 text-slate-700'
}`}>

// Grid layout
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">

// Flex layout
<div className="flex items-center justify-between gap-4">

// Responsive text
<h1 className="text-2xl md:text-3xl lg:text-4xl font-bold">

// Hover effects
<button className="hover:bg-blue-700 transition-colors">

// Status colors
const statusColor = {
  'success': 'bg-green-100 text-green-800',
  'error': 'bg-red-100 text-red-800',
  'pending': 'bg-yellow-100 text-yellow-800',
}
```

## Debugging

```typescript
// Debug log (removal recommendation:)
console.log('[v0] Debug info:', data)

// Error logging
console.error('[v0] Error:', error)

// Network inspection
// DevTools > Network tab > XHR/Fetch

// SignalR debugging
// DevTools > Network tab > WS (WebSocket)
```

## Router Navigation

```typescript
// useNavigate
const navigate = useNavigate()
navigate('/customer/orders')
navigate('/admin/dashboard')

// Link component
<Link to="/customer/create-order" className="...">
  Sipariş Oluştur
</Link>

// Parameterli route
<Route path="/customer/live-tracking/:orderId" element={...} />
const { orderId } = useParams()
```

## Common Patterns

### Sayfa Şablonu
```typescript
const PageTemplate: React.FC = () => {
  const { onAddToast } = props
  const [loading, setLoading] = useState(true)
  const [data, setData] = useState(null)

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    setLoading(true)
    try {
      const res = await api.getData()
      setData(res.data)
    } catch {
      onAddToast('Hata', 'error')
    } finally {
      setLoading(false)
    }
  }

  if (loading) return <LoadingSpinner />

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Title</h1>
        <p className="text-slate-600">Description</p>
      </div>
      {/* Content */}
    </div>
  )
}
```

### API Hatası Yönetimi
```typescript
try {
  const response = await apiCall()
  onAddToast('Başarılı', 'success')
} catch (err: any) {
  const message = err.response?.data?.message || 'Bilinmeyen hata'
  onAddToast(message, 'error')
  console.error('[v0] Error:', err)
}
```

### Form Validation
```typescript
const [errors, setErrors] = useState<Record<string, string>>({})

const validate = (): boolean => {
  const newErrors: Record<string, string> = {}
  if (!form.email) newErrors.email = 'Email gerekli'
  if (form.password.length < 6) newErrors.password = 'Min 6 karakter'
  setErrors(newErrors)
  return Object.keys(newErrors).length === 0
}

const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault()
  if (!validate()) return
  // Submit
}
```

## Production Checklist

- [ ] Environment variables ayarlanmıştır
- [ ] Backend API çalışıyor mu kontrol et
- [ ] Google Maps API key geçerli mi
- [ ] CORS yapılandırması doğru mu
- [ ] SignalR hub'ı çalışıyor mu
- [ ] Database migrations çalıştırılmış mı
- [ ] Build başarılı mı: `pnpm build`
- [ ] Dev server çalışıyor mu: `pnpm dev`

## Sık Sorulan Sorular

**Q: CORS hatası alıyorum**
A: Backend'de CORS yapılandırmasını kontrol et ve Vite proxy'yi doğrula

**Q: SignalR bağlanmıyor**
A: Token süresi dolduysa logout olur. Hub URL'sini kontrol et

**Q: Harita yüklenmedi**
A: Google Maps API key'ini `.env` dosyasına ekle ve API'yi etkinleştir

**Q: Sayfa yüklenmediği görünüyor**
A: DevTools Console'da hata kontrolü yap. Loading state'i verifies et

## Performans İpuçları

1. **Code Splitting**: Sayfaları lazy load et
2. **Image Optimization**: Compressed images kullan
3. **Caching**: API responses'ı cache et
4. **Memoization**: React.memo kullan
5. **Debouncing**: Search input'ları debounce et

## Kaynaklar

- React: https://react.dev
- React Router: https://reactrouter.com
- Tailwind: https://tailwindcss.com
- Vite: https://vitejs.dev
- SignalR: https://learn.microsoft.com/en-us/aspnet/core/signalr

## Destek

Sorun varsa GitHub Issues'i kontrol et veya team'e sor.
