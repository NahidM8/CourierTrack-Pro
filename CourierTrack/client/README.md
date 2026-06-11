# CourierTrack Pro - Kargo Takip Sistemi

Modern bir kargo takip ve teslimat yönetim uygulaması. Müşteriler paketlerini takip edebilir, kuryeler teslimatlarını yönetebilir ve yöneticiler sistemin tamamını kontrol edebilir.

## Teknoloji Stack

- **Frontend**: React 18, TypeScript, React Router v6, Tailwind CSS
- **State Management**: Zustand, Context API
- **API Client**: Axios
- **Real-time Communication**: SignalR (WebSockets)
- **Maps**: Google Maps API
- **Payments**: Stripe
- **Build Tool**: Vite
- **Backend**: .NET (https://localhost:5000)

## Proje Yapısı

```
src/
├── pages/              # Sayfa bileşenleri
│   ├── customer/      # Müşteri sayfaları
│   ├── courier/       # Kurye sayfaları
│   ├── admin/         # Yönetici sayfaları
│   └── *.tsx          # Genel sayfalar
├── components/
│   └── common/        # Ortak bileşenler (Navbar, PrivateRoute, vb.)
├── context/           # Auth ve global state
├── services/
│   ├── api.ts         # API çağrıları
│   └── signalr.ts     # Real-time haberleşme
├── types/             # TypeScript tip tanımları
├── utils/             # Yardımcı fonksiyonlar
└── App.tsx            # Ana uygulama
```

## Başlangıç

### Gereksinimler

- Node.js 18+
- pnpm (veya npm/yarn)
- .NET backend sunucusu (çalışıyor olmalı)

### Kurulum

1. **Bağımlılıkları Yükle**
```bash
pnpm install
```

2. **Environment Değişkenlerini Ayarla**

`.env` dosyasını oluştur ve aşağıdakileri ekle:

```env
VITE_API_BASE_URL=https://localhost:5000
VITE_SIGNALR_HUB_URL=https://localhost:5000/hubs/tracking
VITE_GOOGLE_MAPS_API_KEY=YOUR_GOOGLE_MAPS_API_KEY
VITE_STRIPE_PUBLIC_KEY=YOUR_STRIPE_PUBLIC_KEY
```

3. **Geliştirme Sunucusunu Başlat**
```bash
pnpm dev
```

Uygulama `http://localhost:3000` adresinde açılacaktır.

## Özellikler

### Müşteri Sayfaları

- **Sipariş Oluşturma**: Google Maps üzerinde adres seçimi, paket bilgileri, otomatik fiyat hesaplama
- **Siparişlerim**: Aktif ve geçmiş siparişler listesi, durum filtresi
- **Canlı Takip**: Paket konumunun gerçek zamanlı izlenmesi (SignalR)
- **Takip Sorgusu**: Giriş gerektirmeyen basit takip formu

### Kurye Sayfaları

- **Dashboard**: Bugünkü siparişler, istatistikler, kazanç özeti
- **Gelen Siparişler**: Yeni siparişleri kabul/ret etme
- **Aktif Teslimat**: GPS tabanlı takip, konum güncelleme, durum değiştirme
- **Geçmiş Teslimatlar**: Tamamlanan teslimatlar, kazanç özeti

### Yönetici Sayfaları

- **Dashboard**: KPI'lar, grafikler (Recharts), günlük istatistikler
- **Kurye Yönetimi**: Kurye listesi, konum izleme, aktif/pasif yapma
- **Sipariş Yönetimi**: Tüm siparişler, durum filtresi, manuel kurye atama
- **Canlı Harita**: Tüm aktif kuryelerin gerçek zamanlı konumu
- **Raporlar**: Gelir, teslimat, performans raporları (CSV indirme)

## Kimlik Doğrulama

### Giriş Akışı

1. Kullanıcı email ve şifre ile giriş yapar
2. Backend JWT token döndürür
3. Token `authToken` cookie'sinde saklanır
4. Sonraki isteklerde Authorization header'ında gönderilir

### Token Yönetimi

- Tokens otomatik olarak `js-cookie` ile yönetilir
- Token süresi dolduğunda (401), kullanıcı login sayfasına yönlendirilir
- Logout yapıldığında token silinir

### Role-based Access

```typescript
<PrivateRoute requiredRoles={['CUSTOMER']}>
  <CreateOrderPage />
</PrivateRoute>
```

## Real-time Özellikleri (SignalR)

### Kurye Konumu Güncellemeleri

Kurye konumu otomatik olarak GPS'ten alınır ve her 5 saniyede bir backend'e gönderilir:

```typescript
navigator.geolocation.watchPosition((position) => {
  sendLocationUpdate(position.coords.latitude, position.coords.longitude)
})
```

Müşteriler bu güncellemeleri gerçek zamanlı olarak harita üzerinde görebilirler.

### Sipariş Bildirimleri

- Yeni sipariş atandığında kurye anında bildirim alır
- Sipariş durumu değiştiğinde (alındı, teslimatlandı vb.) tüm ilgili taraflar bildirilir

### Bağlantı Yönetimi

SignalR otomatik olarak:
- Kesilmiş bağlantıları yeniden kurar (exponential backoff)
- Fallback mekanizması sağlar
- Logging seviyesi yapılandırılabilir

## API Entegrasyonu

### Base Configuration

```typescript
// services/api.ts
const client = axios.create({
  baseURL: 'https://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
})

// JWT token otomatik eklenir
client.interceptors.request.use((config) => {
  const token = Cookie.get('authToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})
```

### Tipik Endpoint Örnekleri

```typescript
// Siparişler
POST   /api/orders                 - Sipariş oluştur
GET    /api/orders/:id             - Sipariş detayı
GET    /api/orders/my              - Müşterinin siparişleri
PATCH  /api/orders/:id/status      - Durum güncelle

// Kuryeler
GET    /api/courier/profile        - Profil bilgileri
POST   /api/courier/location       - Konum güncelle
GET    /api/courier/orders/incoming - Gelen siparişler
POST   /api/courier/orders/:id/accept - Siparişi kabul et

// Admin
GET    /api/admin/dashboard        - Dashboard verileri
GET    /api/admin/couriers         - Kurye listesi
GET    /api/admin/orders           - Tüm siparişler
```

## Ödeme Entegrasyonu (Stripe)

### Stripe Kurulumu

1. Stripe hesabı oluştur (https://stripe.com)
2. Public key'ini `.env` dosyasına ekle
3. Backend'de webhook'ları yapılandır

### Ödeme Akışı

```typescript
// Ödeme başlat
const response = await paymentAPI.createCheckout(orderId)
// Stripe Checkout sayfasına yönlendir

// Ödeme onayı
await paymentAPI.confirmPayment(paymentIntentId)
```

## Google Maps Entegrasyonu

### API Key Alımı

1. Google Cloud Console'a git
2. Maps JavaScript API'yi etkinleştir
3. API key'ini `.env` dosyasına ekle

### MapComponent Kullanımı

```typescript
<MapComponent
  center={{ lat: 41.0082, lng: 28.9784 }}
  zoom={13}
  markers={[
    { position: { lat: 41.0, lng: 28.9 }, title: 'Alım Noktası', color: 'green' }
  ]}
  onAddressSelect={(address) => console.log(address)}
  height="400px"
/>
```

## Stil ve Tema

### Tailwind CSS

Tüm stil Tailwind CSS ile yazılmıştır. Renk paletine göz at:

- **Primary**: Blue-600 (#3B82F6)
- **Success**: Green-600 (#10B981)
- **Warning**: Amber-600 (#D97706)
- **Error**: Red-600 (#DC2626)
- **Neutral**: Slate (50-900)

## Build ve Deployment

### Production Build

```bash
pnpm build
```

Optimized dist/ klasörü oluşturulur.

### Vercel'e Deploy

```bash
vercel deploy
```

## Linting ve Format

```bash
pnpm lint
```

## Sorun Giderme

### CORS Hatası
Vite proxy ayarları kontrol et (`vite.config.ts`):
```typescript
server: {
  proxy: {
    '/api': {
      target: 'https://localhost:5000',
      changeOrigin: true,
      secure: false,
    },
  },
}
```

### SignalR Bağlantısı Başarısız
- Backend'de CORS yapılandırılmış mı?
- Hub URL'si `.env` dosyasında doğru mu?
- Token süresi dolmadı mı?

### Google Maps Yüklenmedi
- API key'i doğru girdiysen mi?
- API'yi Google Cloud'da etkinleştirdin mi?
- Domain whitelisted mi?

## İletişim ve Destek

Sorularınız veya önerileri için [GitHub Issues](https://github.com) aracılığıyla iletişime geçin.

## Lisans

MIT
