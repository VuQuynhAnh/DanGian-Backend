# Free Stack — Hướng dẫn dùng công nghệ miễn phí

> Mục tiêu: toàn bộ infrastructure $0/tháng trong giai đoạn phát triển và early production.

---

## Tổng quan Free Stack

| Thành phần | Service | Free tier | Giới hạn |
|-----------|---------|-----------|---------|
| PostgreSQL | **Supabase** | ✅ Free | 500MB DB, 2 projects |
| PostgreSQL (alt) | **Neon** | ✅ Free | 0.5GB, 1 project, auto-pause |
| Backend Deploy | **Railway** | ✅ $5 credit/tháng | ~500h runtime |
| Backend Deploy (alt) | **Render** | ✅ Free | Spin down sau 15p inactive |
| Backend Deploy (alt) | **Fly.io** | ✅ Free | 3 shared VMs, 3GB storage |
| Container Registry | **GHCR** | ✅ Free | Public repo miễn phí |
| CI/CD | **GitHub Actions** | ✅ Free | 2000 phút/tháng |
| Monitoring | **UptimeRobot** | ✅ Free | 50 monitors, 5p interval |
| Error tracking | **Sentry** | ✅ Free | 5000 errors/tháng |
| Logs | **Logtail** (Better Stack) | ✅ Free | 1GB/tháng |

---

## Chi tiết từng service

### 1. PostgreSQL — Supabase (Khuyến nghị)

**Tại sao chọn Supabase:**
- PostgreSQL đầy đủ tính năng, không bị giới hạn features
- Dashboard quản lý DB trực quan
- Có sẵn connection pooling (PgBouncer)
- Free: 500MB, 2 projects, không auto-pause (khác Neon)

**Setup:**
```bash
# Connection string format
postgresql://postgres:[PASSWORD]@db.[PROJECT_REF].supabase.co:5432/postgres

# Với connection pooling (khuyến nghị cho production)
postgresql://postgres.[PROJECT_REF]:[PASSWORD]@aws-0-ap-southeast-1.pooler.supabase.com:6543/postgres
```

**Lưu ý:**
- Dùng `Session mode` pooling cho EF Core migrations
- Dùng `Transaction mode` pooling cho API requests thông thường
- **Không** enable Row Level Security (RLS) — dự án dùng EF Core, không dùng Supabase client trực tiếp

---

### 2. PostgreSQL — Neon (Alternative)

**Khi nào dùng Neon thay Supabase:**
- Cần branching database (mỗi PR có DB riêng — hay cho dev/test)
- Không cần dashboard phức tạp

**Đặc điểm:**
- Auto-pause sau 5 phút không dùng (Supabase không pause)
- Database branching: tạo branch từ main DB cho dev/staging

```bash
postgresql://[USER]:[PASSWORD]@[ENDPOINT].neon.tech/[DB]?sslmode=require
```

---

### 3. Backend Deploy — Railway (Khuyến nghị)

**Tại sao Railway:**
- Deploy từ GitHub repo hoặc Docker image — zero config
- $5 credit miễn phí mỗi tháng ≈ 500h runtime
- Single API → 1 service duy nhất, dùng rất ít credit

**Deploy topology (Single API):**
```
Railway project: dan-gian
└── dangian-api (1 service duy nhất — C# ASP.NET Core)
    ├── REST API: /api/*
    └── SignalR Hub: /hubs/game
```

**railway.json:**
```json
{
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "src/DanGian.Api/Dockerfile"
  },
  "deploy": {
    "startCommand": "dotnet DanGian.Api.dll",
    "restartPolicyType": "ON_FAILURE",
    "healthcheckPath": "/health"
  }
}
```

---

### 4. Backend Deploy — Fly.io (Alternative tốt nhất)

**Tại sao Fly.io tốt hơn Railway cho production:**
- 3 shared-cpu VMs miễn phí (không expire)
- Deploy Docker image trực tiếp
- Persistent volume 3GB miễn phí
- Không spin down như Render

```bash
fly launch --name dan-gian-api
fly deploy
```

---

### 5. Backend Deploy — Render (Alternative)

**Ưu điểm:** Free tier không giới hạn giờ  
**Nhược điểm:** Service sleep sau 15 phút không có request — cold start ~30s

**Trick tránh sleep:** UptimeRobot ping `/health` mỗi 5 phút để giữ service warm.

---

### 6. CI/CD — GitHub Actions

**Free:** 2,000 phút/tháng cho private repo, unlimited cho public repo.

```yaml
# .github/workflows/ci.yml
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal

  deploy:
    needs: build-test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: superfly/flyctl-actions/setup-flyctl@master
      - run: flyctl deploy --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

---

### 7. Monitoring — UptimeRobot + Sentry

**UptimeRobot (Free):**
- 50 monitors, check mỗi 5 phút
- Monitor endpoint: `GET /health`
- Alert qua email khi service down

**Sentry (Free):**
- 5,000 errors/tháng
- Tích hợp ASP.NET Core:

```csharp
// Program.cs
builder.WebHost.UseSentry(o => {
    o.Dsn = builder.Configuration["Sentry__Dsn"];
    o.Environment = builder.Environment.EnvironmentName;
    o.TracesSampleRate = 0.1;
});
```

---

## Environment Variables — Production

```env
# .env.example — KHÔNG commit file .env thật

# Database (Supabase)
ConnectionStrings__DefaultConnection=Host=db.[REF].supabase.co;Database=postgres;Username=postgres;Password=[PASS]

# JWT
Jwt__Secret=<min 32 chars random string>
Jwt__Issuer=dangian-api
Jwt__Audience=dangian-client
Jwt__AccessTokenExpiryMinutes=15

# Zalo OAuth
Zalo__AppId=your-zalo-app-id
Zalo__AppSecret=your-zalo-app-secret

# Sentry (optional)
Sentry__Dsn=https://xxx@sentry.io/xxx

# Environment
ASPNETCORE_ENVIRONMENT=Production
```

---

## Tổng kết chi phí

### Scenario 1: Hoàn toàn miễn phí (dev + early production)

| Service | Cost |
|---------|------|
| Supabase (PostgreSQL) | $0 |
| Fly.io (1 API service) | $0 |
| GitHub Actions CI/CD | $0 |
| GHCR (container registry) | $0 |
| UptimeRobot monitoring | $0 |
| Sentry error tracking | $0 |
| **Tổng** | **$0/tháng** |

### Scenario 2: Scale up khi có user

| Service | Cost |
|---------|------|
| Supabase Pro | $25/tháng |
| Fly.io (scale up) | ~$10-20/tháng |
| **Tổng** | **~$35-45/tháng** |

---

## Recommended Stack

```
PostgreSQL  → Supabase (free, no auto-pause)
Deploy      → Fly.io (1 service, single API)
CI/CD       → GitHub Actions
Registry    → GHCR
Monitor     → UptimeRobot + Sentry
```
