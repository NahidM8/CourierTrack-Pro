# CourierTrack Pro - Setup Rehberi

Bu dokümenta uygulamayı kurulum ve çalıştırma adımlarını bulacaksınız.

## Ön Koşullar

- Node.js 18 veya üzeri
- pnpm (https://pnpm.io)
- .NET Backend sunucusu (https://localhost:5000)
- Google Maps API Key
- Stripe hesabı (opsiyonel - demo için)

## Adım 1: Repository Klonla

```bash
git clone <repository-url>
cd couriertrack-pro
```

## Adım 2: Bağımlılıkları Yükle

```bash
pnpm install
```

## Adım 3: Environment Dosyası Oluştur

`.env` dosyasını oluştur:

```bash
cp .env.example .env
```

Dosyayı düzenle ve gerekli değerleri gir:

```env
# API Ayarları
VITE_API_BASE_URL=https://localhost:5000
VITE_SIGNALR_HUB_URL=https://localhost:5000/hubs/tracking

# Google Maps (https://cloud.google.com/maps-platform)
VITE_GOOGLE_MAPS_API_KEY=YOUR_API_KEY_HERE

# Stripe (https://stripe.com)
VITE_STRIPE_PUBLIC_KEY=pk_test_XXXXXXXXXXXX
```

## Adım 4: Backend Sunucusunu Başlat

Backend'in https://localhost:5000 adresinde çalışıyor olduğundan emin ol.

### Backend Gereksinimleri

Backend'de aşağıdaki konfigürasyonlar olmalı:

1. **CORS Yapılandırması**
```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
```

2. **SignalR Hub**
```csharp
app.MapHub<TrackingHub>("/hubs/tracking");
```

3. **JWT Yapılandırması**
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = "CourierTrackAPI",
            ValidateAudience = true,
            ValidAudience = "CourierTrackApp"
        };
    });
```

## Adım 5: Geliştirme Sunucusunu Başlat

```bash
pnpm dev
```

Uygulama otomatik olarak http://localhost:3000 adresinde açılacaktır.

## Adım 6: Uygulamayı Test Et

### Hesap Oluştur

1. "Kaydol" butonuna tıkla
2. Kullanıcı bilgilerini gir
3. Rol seç (Müşteri veya Kurye)
4. Kaydol butonuna tıkla

### Müşteri Olarak Sipariş Oluştur

1. Giriş yap
2. "Sipariş Oluştur" sayfasına git
3. Alım ve teslimat adreslerini harita üzerinden seç
4. Paket bilgilerini gir
5. Fiyatı hesapla
6. Ödemeye devam et (test kartını kullan)

### Kurye Olarak Teslimat Yönet

1. Kurye hesabı ile giriş yap
2. Dashboard'da bugünkü istatistikleri gör
3. "Gelen Siparişler"den siparişleri kabul et
4. "Aktif Teslimat"de GPS konumunu güncelle

### Yönetici Olarak Sistemi Yönet

1. Admin hesabı ile giriş yap
2. Dashboard'da KPI'ları gör
3. Kuryeleri yönet, konum izle
4. Siparişleri filtrele ve atama yap
5. Raporları oluştur ve indir

## Adım 7: Build et (Production)

```bash
pnpm build
```

Optimized `dist/` klasörü oluşturulacaktır.

## Adım 8: Preview et

```bash
pnpm start
```

## Sorun Giderme

### Port 3000 Zaten Kullanımda

Başka bir port kullan:

```bash
pnpm dev -- --port 3001
```

### CORS Hatası

Vite proxy ayarlarını kontrol et ve backend CORS'unu etkinleştir.

### SignalR Bağlantısı Başarısız

1. Backend'in çalışıp çalışmadığını kontrol et
2. Hub URL'sinin doğru olduğunu doğrula
3. WebSocket'in etkinleştirildiğini kontrol et
4. Tarayıcı geliştirici araçlarında ağ sekmesini kontrol et

### Google Maps Yüklenmedi

1. API key'ini kontrol et
2. Domain'i Google Cloud'da whitelisted yap
3. Maps JavaScript API'yi etkinleştir

### Stripe Hatası

1. Public key'ini kontrol et
2. Test mod'da olduğundan emin ol
3. Backend'de webhook'ları yapılandır

## Veritabanı Migrasyonları

Backend'de veritabanı migrasyonlarını çalıştır:

```bash
dotnet ef database update
```

Gerekli tablolar:
- Users
- Couriers
- Orders
- Addresses
- Packages
- Deliveries
- Payments
- Notifications

## Test Verileri

Admin panelinden test kuryesi ve müşteri oluştur veya seed script'ini çalıştır.

## Deployment

### Vercel'e Deploy

```bash
vercel deploy
```

Environment variables'ları Vercel dashboard'a ekle.

### Docker ile Deploy

```dockerfile
FROM node:18-alpine as builder
WORKDIR /app
COPY package.json pnpm-lock.yaml ./
RUN npm install -g pnpm && pnpm install
COPY . .
RUN pnpm build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## Yapılandırma

### Port Değişikliği

```typescript
// vite.config.ts
server: {
  port: 3001, // Port'u değiştir
}
```

### API Base URL

`.env` dosyasında güncelle:
```env
VITE_API_BASE_URL=https://api.example.com
```

### Loglama Seviyesi

SignalR loglama seviyesini değiştir (`src/services/signalr.ts`):
```typescript
.configureLogging(signalR.LogLevel.Warning) // Info, Warning, Error, None
```

## İleri Düzey Konfigürasyon

### Chunk Size Uyarısı

`vite.config.ts` dosyasında `chunkSizeWarningLimit` ayarı vardır.

### Module Aliasing

`tsconfig.json` dosyasında `@` alias'ı tanımlanmıştır:
```typescript
import { useAuth } from '@/context/AuthContext'
```

## Performans Optimizasyonu

1. **Code Splitting**: Recharts, React Router gibi büyük paketler ayrı chunk'lara bölünmüştür
2. **Lazy Loading**: Sayfalar dinamik import'larla yüklenir
3. **Caching**: API yanıtları tarayıcı cache'inde saklanır

## Monitoring ve Logging

### Client-side Logging

```typescript
console.log("[v0] Debug message:", data)
```

Tüm debug logları `[v0]` prefix'i ile başlar.

### Error Handling

```typescript
try {
  await apiCall()
} catch (err) {
  onAddToast('Error message', 'error')
  console.error('Detailed error:', err)
}
```

## Sonraki Adımlar

1. Backend API dokümantasyonunu oku
2. Database schema'sını oluştur
3. Authentication flow'unu test et
4. Real-time features'ı test et
5. Payment integration'ı yapılandır

## Destek

Sorular veya sorunlar için GitHub Issues açın.

## Lisans

MIT
