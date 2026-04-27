# AI Workflow — Quy trình làm việc với Claude

> Tài liệu này định nghĩa cách Developer và Claude phối hợp xuyên suốt dự án.  
> Đọc kỹ trước khi bắt đầu bất kỳ phase nào.

---

## Nguyên tắc cốt lõi

| # | Nguyên tắc | Lý do |
|---|-----------|-------|
| 1 | **Docs trước, code sau** | Code không có tài liệu → tech debt ngay từ đầu |
| 2 | **Context đầy đủ mỗi session** | Claude không nhớ session trước — thiếu context = output sai |
| 3 | **Không merge code chưa hiểu** | Developer chịu trách nhiệm, không phải AI |
| 4 | **Task nhỏ, rõ ràng** | 1 prompt = 1 việc cụ thể → output tốt hơn nhiều |
| 5 | **Test luôn đi kèm code** | Yêu cầu Claude viết test trong cùng response |
| 6 | **Không tin AI 100% về security** | Luôn chạy security review riêng |

---

## Vòng lặp phát triển chính (Core Loop)

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│  [Bạn] Định nghĩa task + chuẩn bị context           │
│       ↓                                             │
│  [Claude] Sinh code + test + giải thích              │
│       ↓                                             │
│  [Bạn] Review code                                  │
│       ↓                                             │
│  [Claude] Iterate dựa trên feedback                  │
│       ↓                                             │
│  [Bạn] Approve + commit                             │
│       ↓                                             │
│  [GitHub Actions] CI tự động chạy                   │
│       ↓                                             │
│  Nếu fail → [Claude] phân tích + fix               │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## Phase 1 — Tài liệu & Thiết kế

### Mục tiêu
Hoàn thiện toàn bộ tài liệu kỹ thuật trước khi viết một dòng code.

### Danh sách tài liệu cần hoàn thiện

- [x] `PROJECT_CONTEXT.md` — context tổng quan (file này)
- [x] `SYSTEM_DESIGN.md` — kiến trúc hệ thống
- [x] `FREE_STACK.md` — quyết định công nghệ + free tier
- [x] `AI_WORKFLOW.md` — quy trình làm việc với AI
- [ ] `API_SPEC.md` — OpenAPI/Swagger cho tất cả endpoints
- [ ] `DATABASE_SCHEMA.md` — schema SQL đầy đủ với index
- [ ] `GAME_RULES.md` — luật chơi Ô Ăn Quan (input cho game logic)
- [ ] `ADR.md` — Architecture Decision Records
- [ ] `CICD_GUIDE.md` — hướng dẫn setup CI/CD

### Prompt pattern cho phase này

```
Bạn là senior architect đang xây dựng [tên dự án].
Context: [dán PROJECT_CONTEXT.md hoặc đoạn liên quan]

Hãy [tạo / hoàn thiện / review] [tên tài liệu] cho [phần cụ thể].
Yêu cầu:
- [yêu cầu 1]
- [yêu cầu 2]
Format output: Markdown, có thể dùng trực tiếp trong repo.
```

---

## Phase 2 — Setup môi trường

### Checklist

- [ ] Tạo GitHub repo (public hoặc private)
- [ ] Setup Supabase project + lấy connection string
- [ ] Setup Upstash Redis + lấy connection string
- [ ] Setup GitHub Secrets (DATABASE_URL, REDIS_URL, ZALO credentials...)
- [ ] Claude sinh folder structure + base Dockerfiles
- [ ] Claude sinh `docker-compose.yml` cho local dev
- [ ] Claude sinh GitHub Actions workflows
- [ ] Test: `docker compose up` chạy được local
- [ ] Test: CI pipeline chạy xanh với empty project

### Prompt pattern

```
Hãy sinh [docker-compose.yml / Dockerfile / workflow file] cho dự án Dân Gian.
Stack: [liệt kê từ PROJECT_CONTEXT.md]
Yêu cầu:
- Environment variables từ .env file (không hardcode)
- Health check endpoint /health cho mỗi service
- [yêu cầu cụ thể khác]
```

---

## Phase 3 — Phát triển feature

### Quy tắc viết prompt cho code

**Template chuẩn:**
```
Context dự án: [dán đoạn liên quan từ SYSTEM_DESIGN.md hoặc API_SPEC.md]

Task: Implement [tên feature/function cụ thể]

Stack:
- Language: C# / ASP.NET Core 8
- Database: PostgreSQL (Supabase)
- ORM: EF Core 8
- Testing: xUnit + Moq

Yêu cầu:
1. [yêu cầu chức năng]
2. [yêu cầu phi chức năng: performance, security]
3. Follow pattern Repository + Service layer

Output cần:
- File implementation
- File unit test
- Giải thích các quyết định quan trọng
- Checklist review trước khi merge
```

### Ví dụ prompt tốt vs xấu

**Xấu ❌:**
```
Viết game service cho Ô Ăn Quan
```

**Tốt ✅:**
```
Context: [dán phần Game Service từ SYSTEM_DESIGN.md]

Task: Implement GameSession entity và GameSessionRepository
Stack: C# + EF Core 8 + PostgreSQL
Schema: [dán từ DATABASE_SCHEMA.md]

Yêu cầu:
- Entity GameSession với các fields theo schema
- Repository interface + implementation
- Async/await toàn bộ
- Không expose DB entity trực tiếp ra API (dùng DTO)

Output: file entity, repository, unit test với xUnit
```

---

## Phase 4 — Testing

### Yêu cầu coverage

| Loại test | Công cụ | Target |
|-----------|---------|--------|
| Unit test | xUnit + Moq (C#), Jest (Node.js) | ≥ 80% business logic |
| Integration test | xUnit + TestContainers | ≥ 60% API endpoints |
| Game logic test | xUnit | 100% — không exception |

### Prompt pattern cho test

```
Dựa trên implementation sau:
[dán code đã viết]

Hãy viết unit test xUnit cho:
1. Happy path
2. Edge cases: [liệt kê]
3. Error cases: [liệt kê]

Dùng Moq để mock dependencies.
Tên test theo pattern: [MethodName]_[Scenario]_[ExpectedResult]
```

### Prompt pattern cho security review

```
Review security cho đoạn code sau:
[dán code]

Kiểm tra theo OWASP Top 10:
- SQL Injection
- Authentication bypass
- Missing input validation
- Sensitive data exposure
- Insecure direct object reference

Báo cáo theo format: [Issue] → [Risk level] → [Fix]
```

---

## Phase 5 — Deploy

### Checklist deploy lần đầu

- [ ] Build Docker image local thành công
- [ ] Push image lên GHCR
- [ ] Deploy lên Fly.io (hoặc Railway)
- [ ] Kiểm tra health endpoint trả về 200
- [ ] Kết nối Supabase + Upstash thành công
- [ ] Chạy database migration
- [ ] Setup UptimeRobot monitor
- [ ] Setup Sentry error tracking
- [ ] Test end-to-end: Mini App → API → DB

---

## Quản lý context giữa các session

### Vấn đề
Claude không nhớ conversation cũ. Mỗi session mới = bắt đầu lại.

### Giải pháp: Context file chuẩn bị sẵn

Trước mỗi session code, bắt đầu bằng:

```
## Session context

Dự án: Dân Gian (xem PROJECT_CONTEXT.md để biết full context)
Phase hiện tại: [Phase 3 - Feature development]
Service đang làm: [Identity Service]
Task hôm nay: [Implement Zalo OAuth flow]

Files đã có liên quan:
- [dán nội dung file liên quan nếu cần]

Tiếp tục từ: [mô tả trạng thái hiện tại]
```

### Tip: Dùng Claude Project

Tạo một **Claude Project** và upload các file tài liệu vào đó. Claude sẽ tự động có context mà không cần paste mỗi lần.

Files nên có trong Claude Project:
1. `PROJECT_CONTEXT.md`
2. `SYSTEM_DESIGN.md`
3. `API_SPEC.md` (khi hoàn thiện)
4. `DATABASE_SCHEMA.md` (khi hoàn thiện)
5. `GAME_RULES.md` (khi hoàn thiện)

---

## Xử lý khi AI output không đúng

### Tình huống 1: Code không follow tài liệu
```
Code bạn sinh không match với API spec ở [đoạn cụ thể].
Theo spec, endpoint này phải trả về [X] nhưng code đang trả về [Y].
Hãy sửa lại theo đúng spec.
```

### Tình huống 2: Code có bug
```
Code này có bug: [mô tả bug + stack trace nếu có]
Đây là behavior hiện tại: [X]
Expected behavior: [Y]
Hãy phân tích nguyên nhân và fix.
```

### Tình huống 3: Cần giải thích
```
Tôi chưa hiểu đoạn code này:
[dán đoạn code]
Giải thích:
1. Nó làm gì?
2. Tại sao chọn approach này thay vì [alternative]?
3. Risk/trade-off là gì?
```

---

## Thứ tự xây dựng đề xuất

```
Tuần 1-2: Hoàn thiện tài liệu
├── API_SPEC.md
├── DATABASE_SCHEMA.md  
├── GAME_RULES.md
└── ADR.md

Tuần 3: Setup hạ tầng
├── GitHub repo + secrets
├── Supabase + Upstash setup
├── Docker + docker-compose local
└── GitHub Actions CI (empty project)

Tuần 4-5: Identity Service
├── User entity + migration
├── Zalo OAuth flow
├── JWT issue/refresh
└── Unit test + integration test

Tuần 6-7: Game Service (Ô Ăn Quan logic)
├── GameSession entity + repository
├── Game logic engine (pure functions)
├── Solo mode vs AI
└── Unit test game rules (100% coverage)

Tuần 8: Realtime Service
├── Socket.IO setup
├── Room management
├── Game state sync
└── Reconnection handling

Tuần 9: Mission + Leaderboard
├── Daily mission system
├── Points calculation
└── Redis leaderboard

Tuần 10: Integration + Deploy
├── End-to-end test
├── Deploy lên Fly.io
└── Mini App kết nối API thật
```
