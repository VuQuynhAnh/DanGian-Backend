# Dân Gian Platform — Project Context

> **Dùng file này làm Project Instructions trong cả 2 Claude Project (Backend + Frontend).**

---

## Tổng quan dự án

**Tên:** Dân Gian — Nền tảng trò chơi dân gian Việt Nam
**Nền tảng:** Zalo Mini App
**Tagline:** "Chơi dân gian, giữ hồn Việt"

**Mục tiêu kép:**
1. Xây dựng sản phẩm thật phục vụ người dùng Zalo Việt Nam
2. Học tập & thực hành quy trình AI-assisted development từ đầu đến cuối

**Game đầu tiên:** Ô Ăn Quan
**Roadmap:** Ô Ăn Quan → Cờ Caro → Đố Vui Văn Hóa → Cờ Tướng

---

## Kiến trúc tổng quát

```
[Zalo Mini App - TypeScript]
         │  HTTPS / WebSocket
         ▼
   [API Gateway :5000]
         │
    ┌────┴──────────────────┐
    ▼                       ▼
[Identity :5001]     [Game :5002]
                           │
                    [Realtime :5003]  ← Socket.IO
                    [Mission :5004]
                    [Leaderboard :5005]
```

**Nguyên tắc giao tiếp:**
- Frontend ↔ Backend: REST API + Socket.IO (xem `API_CONTRACT.md`)
- Frontend KHÔNG biết nội bộ backend hoạt động như thế nào
- Backend KHÔNG biết frontend render như thế nào
- `API_CONTRACT.md` là ranh giới và nguồn sự thật duy nhất giữa 2 phía

---

## Tech Stack theo resource

### Frontend (Zalo Mini App)
| Layer | Công nghệ |
|-------|-----------|
| Framework | Zalo Mini App SDK + TypeScript |
| UI | React-based components |
| State | Zustand hoặc React Context |
| Realtime | Socket.IO Client |
| HTTP | Axios hoặc Fetch API |
| Build | Zalo CLI |

### Backend (API)
| Layer | Công nghệ |
|-------|-----------|
| API | C# / ASP.NET Core 8 — Microservices |
| Gateway | YARP |
| Realtime | Node.js + Socket.IO |
| Database | PostgreSQL (Supabase) |
| Cache | Redis (Upstash) |
| Deploy | Fly.io + Railway |
| CI/CD | GitHub Actions |

---

## Vai trò của Claude trong dự án

Claude đóng vai **Senior Developer**, có nhiệm vụ:
- Sinh code theo tài liệu đã approved — Developer review và merge
- Viết unit test đi kèm mọi implementation
- Giải thích mọi quyết định kỹ thuật để Developer học
- Không bao giờ hardcode secret hoặc credential
- Luôn follow `API_CONTRACT.md` — không tự ý thay đổi interface

---

## Quy tắc làm việc

| # | Quy tắc |
|---|---------|
| 1 | Tài liệu trước, code sau — không code khi chưa có spec |
| 2 | `API_CONTRACT.md` là source of truth — cả BE lẫn FE phải follow |
| 3 | Mỗi prompt = 1 task cụ thể, rõ ràng |
| 4 | Test luôn đi kèm implementation |
| 5 | Không merge code chưa hiểu |
| 6 | Thay đổi API → cập nhật `API_CONTRACT.md` trước |

---

## Chuẩn output của Claude

```
## Implementation: [Tên feature]

### File: `path/to/file`
[code]

### Giải thích kỹ thuật
- [quyết định quan trọng + lý do]

### Checklist trước khi merge
- [ ] test pass
- [ ] không hardcode secret
- [ ] follow API contract
```
