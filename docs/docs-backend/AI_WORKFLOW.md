# AI Workflow — Quy trình làm việc với Claude

> Tài liệu này định nghĩa cách Developer và Claude phối hợp xuyên suốt dự án.

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

## Context mỗi session (paste vào đầu session)

```
Dự án: Dân Gian Backend
Stack: C# 12 / .NET 8, Clean Architecture + DDD + CQRS (MediatR 14), EF Core 8
Pattern: Controller → Sender.Send(Command) → Pipeline Behaviors → Handler → Repository → Result<T>
Docs: SYSTEM_DESIGN.md (kiến trúc), API_CONTRACT.md (endpoints), DATABASE_SCHEMA.md (schema)

Task hôm nay: [mô tả cụ thể]
Context liên quan: [dán đoạn doc hoặc code nếu cần]
```

---

## Phase 3 — Phát triển feature (CQRS pattern)

### Template prompt chuẩn

```
Stack: C# 12 / .NET 8, Clean Architecture + CQRS (MediatR 14), EF Core 8, FluentValidation 12

Context:
- Bounded context: [Identity | Game | Mission | Leaderboard]
- [Dán phần liên quan từ API_CONTRACT.md hoặc DATABASE_SCHEMA.md]

Task: Implement [tên feature]

Yêu cầu:
1. Command/Query record + Handler (internal sealed) + Validator + Response record
2. Đặt tại: src/DanGian.Application/Features/{Context}/{Commands|Queries}/{Name}/
3. Handler dùng Result.Failure() — không throw exception
4. Repository method nếu chưa có (interface ở Domain, implementation ở Infrastructure)
5. Controller endpoint trong src/DanGian.Api/Controllers/ kế thừa BaseApiController

Output cần:
- Tất cả file implementation
- Unit test xUnit + Moq cho Handler
- Giải thích quyết định kỹ thuật
- Checklist review
```

### Ví dụ prompt tốt vs xấu

**Xấu ❌:**
```
Viết API tạo phòng chơi
```

**Tốt ✅:**
```
Stack: C# 12 / .NET 8, Clean Architecture + CQRS, MediatR 14, EF Core 8

Context (từ API_CONTRACT.md):
POST /games/{gameType}/rooms → tạo phòng bạn bè
Response: { roomId, roomCode, gameType, hostId, players[], status }

Task: Implement CreateRoomCommand

Yêu cầu:
- Command: CreateRoomCommand(Guid UserId, string GameType) : ICommand<CreateRoomResponse>
- Handler: tạo Room aggregate, sinh roomCode 6 ký tự unique, lưu qua IRoomRepository
- Validator: GameType không rỗng, phải là "o_an_quan" hoặc "co_caro"
- Response record: CreateRoomResponse(Guid RoomId, string RoomCode, ...)
- Controller: RoomsController.CreateRoom() → HandleResult(await Sender.Send(...))

Schema Room (từ DATABASE_SCHEMA.md):
[dán phần CREATE TABLE game.rooms]

Output: tất cả file + unit test Handler
```

---

## Phase 4 — Testing

### Template prompt test

```
Dựa trên Handler sau (dán code):

Viết unit test xUnit + Moq cho LoginCommandHandler:
1. Happy path: user chưa tồn tại → tạo mới → trả LoginResponse có AccessToken
2. Happy path: user đã tồn tại → cập nhật profile → trả LoginResponse
3. Edge case: ZaloId rỗng → ValidationBehavior chặn (test Validator riêng)

Mock: IUserRepository, IUnitOfWork, IJwtTokenGenerator
Naming: Handle_[Scenario]_[ExpectedResult]
```

### Template prompt security review

```
Review security cho đoạn code sau:
[dán code]

Kiểm tra:
- SQL Injection (EF Core raw query?)
- Input validation thiếu không?
- Sensitive data bị log không?
- JWT claims được validate đủ không?
- Unauthorized access — thiếu [Authorize]?

Báo cáo: [Issue] → [Risk level: High/Medium/Low] → [Fix]
```

---

## Xử lý khi AI output không đúng

### Code không follow pattern CQRS
```
Code bạn sinh dùng Service layer thay vì Handler. Dự án này dùng CQRS với MediatR:
- Không có IService — chỉ có ICommandHandler và IQueryHandler
- Controller gọi Sender.Send(), không inject service
- Handler trả Result<T>, không throw exception
Hãy viết lại đúng pattern.
```

### Code có bug
```
Code này có bug: [mô tả + stack trace]
Behavior hiện tại: [X]
Expected behavior: [Y]
Hãy phân tích nguyên nhân và fix.
```

### Cần giải thích
```
Tôi chưa hiểu đoạn code này:
[dán code]
Giải thích:
1. Nó làm gì trong luồng CQRS?
2. Tại sao không throw exception mà dùng Result.Failure?
3. UnitOfWork ở đây có vai trò gì?
```

---

## Thứ tự xây dựng

```
Sprint 1: Identity Context
├── LoginCommand (JWT issue từ ZaloId)
└── GetProfileQuery

Sprint 2: Game Context — Solo
├── CreateSessionCommand
├── MakeMoveCommand (game logic engine)
└── GameHub (SignalR real-time)

Sprint 3: Mission Context
├── GetDailyMissionsQuery
└── ClaimMissionCommand

Sprint 4: Leaderboard Context
└── GetLeaderboardQuery

Sprint 5: Game Context — PvP
├── CreateRoomCommand
├── JoinRoomCommand
└── Matchmaking (ranked)
```
