# System Design — Backend

> Tài liệu nội bộ Backend. Frontend không cần biết nội dung này.

---

## Microservices overview

| Service | Port | Runtime | Trách nhiệm |
|---------|------|---------|-------------|
| API Gateway | 5000 | C# YARP | Route, JWT validate, rate limit |
| Identity Service | 5001 | C# ASP.NET Core 8 | Zalo OAuth, JWT, user CRUD |
| Game Service | 5002 | C# ASP.NET Core 8 | Game logic, sessions, history |
| Realtime Service | 5003 | Node.js 20 + Socket.IO | WebSocket, room management |
| Mission Service | 5004 | C# ASP.NET Core 8 | Daily missions, points |
| Leaderboard Service | 5005 | C# ASP.NET Core 8 | Rankings, seasons |

---

## Giao tiếp nội bộ

```
Client request
   │
   ▼
API Gateway (JWT validate tại đây — 1 lần duy nhất)
   │
   ├── /auth/*       → Identity Service (HTTP)
   ├── /users/*      → Identity Service (HTTP)
   ├── /games/*      → Game Service (HTTP)
   ├── /missions/*   → Mission Service (HTTP)
   ├── /leaderboard/* → Leaderboard Service (HTTP)
   └── /socket.io    → Realtime Service (WebSocket proxy)

Service-to-service (async):
   Game Service ──[Redis Pub/Sub]──→ Mission Service (game ended event)
   Game Service ──[Redis Pub/Sub]──→ Leaderboard Service (score update)
   Mission Service ──[Redis Pub/Sub]──→ Realtime Service (mission notification)
```

---

## Redis key patterns

| Key | Type | TTL | Dùng cho |
|-----|------|-----|---------|
| `session:{userId}` | Hash | 7 ngày | Refresh token store |
| `ratelimit:{userId}:{endpoint}` | Counter | 1 phút | Rate limiting |
| `room:{roomId}` | JSON | 2 giờ | Room state |
| `room:{roomId}:players` | Set | 2 giờ | Player IDs trong phòng |
| `game:{sessionId}:state` | JSON | 1 giờ | Game state realtime |
| `queue:ranked:{gameType}` | List | - | Matchmaking queue |
| `leaderboard:weekly:{weekKey}` | Sorted Set | 7 ngày | Weekly rankings |
| `leaderboard:season:{seasonId}` | Sorted Set | 90 ngày | Season rankings |
| `mission:{userId}:{date}` | Hash | 2 ngày | Daily mission progress |
| `user:profile:{userId}` | JSON | 5 phút | Profile cache |

---

## Redis Pub/Sub channels

| Channel | Publisher | Subscriber | Payload |
|---------|-----------|------------|---------|
| `game.ended` | Game Service | Mission, Leaderboard | `{sessionId, players, scores, gameType}` |
| `mission.completed` | Mission Service | Realtime | `{userId, missionId, reward}` |
| `score.updated` | Mission Service | Leaderboard | `{userId, delta, newTotal}` |
| `match.found` | Game Service | Realtime | `{sessionId, player1Id, player2Id}` |

---

## Clean Architecture (mỗi C# service)

```
ServiceName/
├── Controllers/          ← HTTP endpoints, validate input, trả response
├── Services/
│   ├── Interfaces/       ← IGameService, IMissionService...
│   └── Implementations/  ← Business logic
├── Repositories/
│   ├── Interfaces/       ← IGameSessionRepository...
│   └── Implementations/  ← EF Core queries
├── Models/
│   ├── Entities/         ← EF Core entities (map với DB)
│   ├── DTOs/             ← Request/Response objects
│   └── Enums/
├── Middleware/           ← Exception handler, logging...
├── Extensions/           ← DI registration, builder extensions
└── Migrations/           ← EF Core migrations
```

**Layer rule:** Controller → Service → Repository → Entity
- Service KHÔNG gọi trực tiếp DbContext
- Controller KHÔNG chứa business logic
- Entity KHÔNG expose ra ngoài — dùng DTO

---

## JWT Strategy

```
Access Token:  15 phút — dùng cho mọi API call
Refresh Token: 7 ngày  — lưu trên Redis, rotate sau mỗi lần refresh

Claims trong Access Token:
  sub      = userId (UUID)
  zalo_id  = Zalo user ID
  role     = "user" | "admin"
  iat, exp

API Gateway validate JWT → inject userId vào header X-User-Id
Các service downstream đọc X-User-Id — không validate JWT lại
```

---

## Error handling chuẩn (C#)

```csharp
// Custom exception hierarchy
DanGianException (base)
├── ValidationException      → 422
├── NotFoundException        → 404
├── UnauthorizedException    → 401
├── ForbiddenException       → 403
├── ConflictException        → 409
└── GameException
    ├── InvalidMoveException → 422
    ├── RoomFullException    → 409
    └── GameNotFoundException→ 404

// Global middleware bắt exception → format response chuẩn theo API_CONTRACT
```

---

## Deployment topology

```
VPS / Fly.io / Railway
├── nginx (reverse proxy, SSL termination)
│   ├── api.dangian.app → api-gateway:5000
│   └── wss://api.dangian.app/socket.io → realtime-service:5003
├── api-gateway container
├── identity-service container
├── game-service container
├── realtime-service container
├── mission-service container
└── leaderboard-service container

External (free tier):
├── Supabase → PostgreSQL
└── Upstash  → Redis
```

---

## Docker Compose (local dev)

```yaml
# docker-compose.yml tóm tắt
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: dangian
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    ports: ["5432:5432"]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

  api-gateway:
    build: ./apps/api-gateway
    ports: ["5000:5000"]
    depends_on: [postgres, redis]
    environment:
      - IDENTITY_SERVICE_URL=http://identity-service:5001
      - GAME_SERVICE_URL=http://game-service:5002

  identity-service:
    build: ./apps/identity-service
    ports: ["5001:5001"]
    environment:
      - DATABASE_URL=${DATABASE_URL}
      - JWT_SECRET=${JWT_SECRET}
      - ZALO_APP_ID=${ZALO_APP_ID}
      - ZALO_APP_SECRET=${ZALO_APP_SECRET}
  # ... các service khác tương tự
```
