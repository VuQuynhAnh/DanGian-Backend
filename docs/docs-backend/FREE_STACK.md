# Free Stack — Hướng dẫn dùng công nghệ miễn phí

> Mục tiêu: toàn bộ infrastructure $0/tháng trong giai đoạn phát triển và early production.

---

## Tổng quan Free Stack

| Thành phần | Service | Free tier | Giới hạn |
|-----------|---------|-----------|---------|
| PostgreSQL | **Supabase** | ✅ Free | 500MB DB, 2 projects |
| PostgreSQL (alt) | **Neon** | ✅ Free | 0.5GB, 1 project, auto-pause |
| Redis | **Upstash** | ✅ Free | 10,000 cmd/ngày, 256MB |
| Backend Deploy | **Railway** | ✅ $5 credit/tháng | ~500h runtime |
| Backend Deploy (alt) | **Render** | ✅ Free | Spin down sau 15p inactive |
| Backend Deploy (alt) | **Fly.io** | ✅ Free | 3 shared VMs, 3GB storage |
| Container Registry | **GHCR** | ✅ Free | Public repo miễn phí |
| CI/CD | **GitHub Actions** | ✅ Free | 2000 phút/tháng |
| Realtime | **Railway** hoặc tự host | ✅ | Cùng với backend |
| Domain | **js.org** hoặc subdomain | ✅ Free | Cần GitHub repo |
| SSL | **Let's Encrypt** | ✅ Free | Auto-renew |
| Monitoring | **Better Uptime** (free) | ✅ Free | 1 monitor, ping 3p |
| Monitoring (alt) | **UptimeRobot** | ✅ Free | 50 monitors, 5p interval |
| Error tracking | **Sentry** | ✅ Free | 5000 errors/tháng |
| Logs | **Logtail** (Better Stack) | ✅ Free | 1GB/tháng |

---

## Chi tiết từng service

### 1. PostgreSQL — Supabase (Khuyến nghị)

**Tại sao chọn Supabase:**
- PostgreSQL đầy đủ tính năng, không bị giới hạn features
- Dashboard quản lý DB trực quan
- Có sẵn connection pooling (PgBouncer)
- REST API tự động (bonus, không bắt buộc dùng)
- Free: 500MB, 2 projects, không auto-pause (khác Neon)

**Setup:**
```bash
# Connection string format
postgresql://postgres:[PASSWORD]@db.[PROJECT_REF].supabase.co:5432/postgres

# Với connection pooling (khuyến nghị cho production)
postgresql://postgres.[PROJECT_REF]:[PASSWORD]@aws-0-ap-southeast-1.pooler.supabase.com:6543/postgres
```

**Lưu ý:**
- Dùng `Session mode` pooling cho migrations
- Dùng `Transaction mode` pooling cho API requests
- Enable Row Level Security (RLS) nếu dùng Supabase client trực tiếp

---

### 2. PostgreSQL — Neon (Alternative)

**Khi nào dùng Neon thay Supabase:**
- Cần branching database (mỗi PR có DB riêng — rất hay cho dev)
- Không cần dashboard phức tạp

**Đặc điểm:**
- Auto-pause sau 5 phút không dùng (Supabase không pause)
- Serverless PostgreSQL — scale to zero
- Database branching: tạo branch từ main DB cho dev/staging

```bash
# Connection string
postgresql://[USER]:[PASSWORD]@[ENDPOINT].neon.tech/[DB]?sslmode=require
```

---

### 3. Redis — Upstash (Khuyến nghị)

**Tại sao Upstash:**
- Serverless Redis — trả theo request, không theo giờ
- Free: 10,000 req/ngày, 256MB data
- Hỗ trợ Redis Pub/Sub (cần cho Socket.IO adapter)
- Có REST API — dùng được từ edge functions

**Lưu ý quan trọng:**
- 10,000 req/ngày có thể không đủ nếu game active nhiều
- Monitor usage sớm, có plan paid $10/tháng nếu cần
- Dùng Redis cho: session, leaderboard sorted set, game state cache, pub/sub
- Không dùng Redis để store permanent data

```bash
# Connection
REDIS_URL=rediss://:[PASSWORD]@[ENDPOINT].upstash.io:6379
```

---

### 4. Backend Deploy — Railway (Khuyến nghị)

**Tại sao Railway:**
- Deploy từ GitHub repo hoặc Docker image — zero config
- $5 credit miễn phí mỗi tháng ≈ 500h runtime cho 1 service nhỏ
- Nhiều service trong 1 project
- Có sẵn PostgreSQL và Redis add-on (nhưng dùng Supabase + Upstash free tốt hơn)

**Chiến lược tối ưu free tier:**
```
Railway project: dan-gian
├── api-gateway (C# service)
├── identity-service (C# service)  
├── game-service (C# service)
├── realtime-service (Node.js)
├── mission-service (C# service)
└── leaderboard-service (C# service)
```

Tất cả service dùng chung $5 credit. Với traffic nhỏ ban đầu, 6 services nhỏ ≈ $2-4/tháng.

**Deploy từ Docker:**
```yaml
# railway.json
{
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "apps/identity-service/Dockerfile"
  },
  "deploy": {
    "startCommand": "dotnet IdentityService.dll",
    "restartPolicyType": "ON_FAILURE"
  }
}
```

---

### 5. Backend Deploy — Render (Alternative)

**Ưu điểm:** Free tier không giới hạn giờ (nhưng spin down)  
**Nhược điểm:** Service bị sleep sau 15 phút không có request — cold start ~30s

**Phù hợp cho:** staging environment, service ít traffic

**Trick tránh sleep:**
```javascript
// Dùng UptimeRobot ping /health endpoint mỗi 5 phút
// Giữ service luôn warm
```

---

### 6. Fly.io (Alternative tốt nhất cho production miễn phí)

**Tại sao Fly.io tốt hơn Railway/Render cho production:**
- 3 shared-cpu VMs miễn phí (không expire)
- Deploy Docker image trực tiếp
- Có persistent volume 3GB miễn phí
- Global edge network
- Không spin down như Render

```bash
# Install flyctl
curl -L https://fly.io/install.sh | sh

# Deploy
fly launch --name dan-gian-identity
fly deploy
```

**Chiến lược:** dùng Fly.io cho services quan trọng (identity, game), Railway cho services phụ.

---

### 7. CI/CD — GitHub Actions

**Free:** 2,000 phút/tháng cho private repo, unlimited cho public repo.

**Tối ưu phút:**
- Chỉ chạy full CI khi có PR vào `main`
- Feature branch chỉ chạy lint + build (bỏ test nặng)
- Cache dependencies để build nhanh hơn
- Dùng matrix build chỉ khi cần thiết

```yaml
# .github/workflows/ci.yml — tối ưu cho free tier
on:
  push:
    branches: [main, staging]
  pull_request:
    branches: [main]

jobs:
  lint-build:
    if: github.event_name == 'push' && github.ref != 'refs/heads/main'
    # Chỉ lint + build cho feature push
    
  full-ci:
    if: github.event_name == 'pull_request' || github.ref == 'refs/heads/main'
    # Full test cho PR và main
```

---

### 8. Monitoring — UptimeRobot + Sentry

**UptimeRobot (Free):**
- 50 monitors, check mỗi 5 phút
- Alert qua email khi service down
- Setup: monitor `/health` endpoint của mỗi service

**Sentry (Free):**
- 5,000 errors/tháng — đủ cho giai đoạn đầu
- SDK cho C# và Node.js
- Track unhandled exceptions tự động

```csharp
// Program.cs
builder.Services.AddSentry(o => {
    o.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
    o.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
});
```

---

## Tổng kết chi phí

### Scenario 1: Hoàn toàn miễn phí (dev + early production)

| Service | Cost |
|---------|------|
| Supabase (PostgreSQL) | $0 |
| Upstash (Redis) | $0 |
| Fly.io (3 services) | $0 |
| Railway (3 services còn lại) | ~$0-2 (dùng $5 credit) |
| GitHub Actions CI/CD | $0 |
| GHCR (container registry) | $0 |
| UptimeRobot monitoring | $0 |
| Sentry error tracking | $0 |
| **Tổng** | **$0 - $2/tháng** |

### Scenario 2: Scale up khi có user (vẫn rẻ)

| Service | Cost |
|---------|------|
| Supabase Pro | $25/tháng |
| Upstash Pay-as-you-go | ~$5-10/tháng |
| Fly.io (scale up) | ~$10-20/tháng |
| **Tổng** | **~$40-55/tháng** |

---

## Quyết định cuối cùng (Recommended Stack)

```
PostgreSQL  → Supabase (free, no auto-pause)
Redis       → Upstash (free, monitor usage)
Deploy      → Fly.io cho identity + game + realtime
              Railway cho mission + leaderboard + gateway
CI/CD       → GitHub Actions
Registry    → GHCR
Monitor     → UptimeRobot + Sentry
```

---

## Environment Variables Template

```env
# .env.example — KHÔNG commit file .env thật

# Database
DATABASE_URL=postgresql://user:pass@host:5432/dbname

# Redis
REDIS_URL=rediss://:pass@endpoint.upstash.io:6379

# JWT
JWT_SECRET=your-256-bit-secret-here
JWT_ISSUER=dan-gian-api
JWT_AUDIENCE=dan-gian-miniapp
ACCESS_TOKEN_EXPIRY_MINUTES=15
REFRESH_TOKEN_EXPIRY_DAYS=7

# Zalo OAuth
ZALO_APP_ID=your-zalo-app-id
ZALO_APP_SECRET=your-zalo-app-secret
ZALO_REDIRECT_URI=https://your-domain/api/v1/auth/zalo/callback

# Sentry
SENTRY_DSN=https://xxx@sentry.io/xxx

# Environment
ASPNETCORE_ENVIRONMENT=Production
```
