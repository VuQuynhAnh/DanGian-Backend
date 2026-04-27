# SKILL — Backend (C# / ASP.NET Core)

> Upload file này vào Claude Project Backend.
> Áp dụng cho mọi coding task trong dự án Dân Gian — Backend.

---

## Stack bắt buộc

- **Runtime:** C# 12 / .NET 8 (ASP.NET Core)
- **Realtime service:** Node.js 20 + TypeScript + Socket.IO 4
- **ORM:** EF Core 8 (PostgreSQL provider: Npgsql)
- **Validation:** FluentValidation
- **Testing:** xUnit + Moq + TestContainers
- **Logging:** Serilog → stdout (Docker-friendly)
- **Mapping:** Mapster (hoặc manual mapping — không dùng AutoMapper)

---

## Conventions bắt buộc

### Naming
```csharp
// Classes, Methods, Properties: PascalCase
// Parameters, local variables: camelCase
// Private fields: _camelCase
// Constants: UPPER_SNAKE_CASE
// Interfaces: tiền tố I (IGameRepository)
// DTOs: suffix Request/Response (CreateGameRequest, GameStateResponse)
// Exceptions: suffix Exception (InvalidMoveException)
```

### Architecture
```
Controller  → nhận HTTP, validate input, gọi Service, trả DTO
Service     → business logic, gọi Repository, throw exceptions
Repository  → EF Core queries, không có business logic
Entity      → EF Core model, KHÔNG expose ra ngoài Controller
DTO         → Request/Response objects, tách hoàn toàn khỏi Entity
```

### Async
```csharp
// LUÔN async/await cho DB và I/O
// LUÔN nhận CancellationToken
// KHÔNG dùng .Result hoặc .Wait() — deadlock risk
// KHÔNG dùng async void — dùng async Task

public async Task<GameSession> CreateAsync(
    CreateSessionRequest request,
    CancellationToken ct = default)
{
    // ...
}
```

### Error handling
```csharp
// Dùng custom exceptions, không return null cho "not found"
// Global middleware bắt exception → format theo API_CONTRACT.md

public async Task<GameSession> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    var session = await _context.Sessions
        .FirstOrDefaultAsync(s => s.Id == id, ct);

    return session ?? throw new NotFoundException($"Session {id} not found");
}
```

### Không hardcode
```csharp
// LUÔN đọc từ IConfiguration hoặc Environment
var secret = _config["JWT:Secret"]
    ?? throw new InvalidOperationException("JWT:Secret not configured");

// KHÔNG BAO GIỜ
var secret = "my-secret-key-hardcoded"; // ❌
```

---

## Output format bắt buộc

```markdown
## Implementation: [Tên feature]

### File: `apps/[service]/[path/to/file.cs]`
\```csharp
// code
\```

### File: `apps/[service]/Tests/Unit/[path/to/test.cs]`
\```csharp
// xUnit test
\```

### Giải thích kỹ thuật
- **[Quyết định]:** lý do

### Checklist trước khi merge
- [ ] Unit test pass (xunit)
- [ ] Không hardcode credential
- [ ] Input validation đầy đủ (FluentValidation)
- [ ] Async/await + CancellationToken
- [ ] Logging đủ (không log sensitive data)
- [ ] Follow API_CONTRACT.md response format
```

---

## Patterns chuẩn trong dự án

### Repository Pattern
```csharp
public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<GameSession> CreateAsync(GameSession session, CancellationToken ct = default);
    Task UpdateAsync(GameSession session, CancellationToken ct = default);
    Task<IReadOnlyList<GameSession>> GetByPlayerIdAsync(
        Guid playerId, int page, int limit, CancellationToken ct = default);
}
```

### Result Pattern (business logic)
```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? ErrorCode { get; init; }

    public static Result<T> Ok(T value) =>
        new() { IsSuccess = true, Value = value };

    public static Result<T> Fail(string errorCode) =>
        new() { IsSuccess = false, ErrorCode = errorCode };
}
```

### Response format (theo API_CONTRACT.md)
```csharp
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public ApiMeta Meta { get; init; } = new();
}

public record ApiMeta
{
    public string Timestamp { get; init; } = DateTime.UtcNow.ToString("O");
}

public record ApiError(string Code, string Message);
```

### Socket.IO events (Node.js realtime service)
```typescript
// Typed events — luôn dùng interface
interface ServerToClientEvents {
  'game:state': (data: GameStateEvent) => void;
  'game:end': (data: GameEndEvent) => void;
  'game:start': (data: GameStartEvent) => void;
  'room:updated': (data: RoomUpdatedEvent) => void;
  'ranked:matched': (data: RankedMatchedEvent) => void;
  'error': (data: ErrorEvent) => void;
}

interface ClientToServerEvents {
  'game:move': (
    data: MakeMoveRequest,
    callback: (res: MakeMoveResponse) => void
  ) => void;
  'room:join': (data: { roomId: string }) => void;
  'room:ready': (data: { roomId: string }) => void;
  'room:leave': (data: { roomId: string }) => void;
  'ranked:queue': (data: { gameType: string }) => void;
}
```

---

## Test naming convention
```csharp
// [MethodName]_[Scenario]_[ExpectedResult]
public async Task MakeMove_ValidCell_ReturnsUpdatedState() { }
public async Task MakeMove_EmptyCell_ThrowsInvalidMoveException() { }
public async Task GetById_NonExistentId_ThrowsNotFoundException() { }
public void CalculateScore_AllCaptured_ReturnsCorrectTotal() { }
```

---

## Security checklist (tự động kiểm tra mọi PR)

- [ ] Không hardcode secret / credential / connection string
- [ ] Input validation với FluentValidation tại mọi endpoint
- [ ] EF Core parameterized query (không raw string concat)
- [ ] JWT claims được validate đầy đủ (issuer, audience, expiry)
- [ ] Authorization attribute trên mọi endpoint cần auth
- [ ] Sensitive data không log (password, token, zalo_secret)
- [ ] Rate limiting áp dụng cho endpoint public
- [ ] HTTPS-only cho production (HSTS header)

---

## Lỗi phổ biến cần tránh

| Lỗi | Cách đúng |
|-----|-----------|
| `async void` | `async Task` |
| `.Result` / `.Wait()` | `await` |
| Catch `Exception` quá rộng | Catch exception cụ thể |
| N+1 query | `.Include()` hoặc explicit join |
| Thiếu index trên FK | Luôn index FK và WHERE columns |
| Magic string | Constant hoặc enum |
| Không dispose DbContext | DI scoped lifetime hoặc `using` |
| Return null thay vì throw | Throw `NotFoundException` |
