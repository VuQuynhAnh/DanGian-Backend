# System Design — Backend

> Tài liệu nội bộ Backend. Đây là source of truth về kiến trúc thực tế của dự án.

---

## Kiến trúc tổng quan

**Single API** — Clean Architecture + Domain Driven Design (DDD) + CQRS (MediatR)

```
src/
  DanGian.Domain/          ← Entities, ValueObjects, Interfaces, Domain Events
  DanGian.Application/     ← CQRS (MediatR), Use Cases, Validators, Pipeline Behaviors
  DanGian.Infrastructure/  ← EF Core, Repositories, JWT, UnitOfWork
  DanGian.Api/             ← Controllers, SignalR Hubs, Middleware, Program.cs
tests/
  DanGian.UnitTests/       ← Domain + Application tests
  DanGian.IntegrationTests/← API integration tests
```

---

## Luồng xử lý một HTTP Request

```
HTTP Request
    ↓
GlobalExceptionMiddleware          ← bắt mọi unhandled exception → ApiResponse<T>
    ↓
Authentication/Authorization       ← JWT Bearer validation
    ↓
Controller (kế thừa BaseApiController)
    ↓
Sender.Send(ICommand | IQuery)     ← MediatR dispatch
    ↓
LoggingBehavior                    ← log tên request/response
    ↓
ValidationBehavior                 ← chạy FluentValidation, throw nếu lỗi
    ↓
CommandHandler | QueryHandler      ← business logic
    ↓
Repository → EF Core → PostgreSQL
    ↓
Result<T>  ←────────────────────── trả về
    ↓
BaseApiController.HandleResult()   ← map Result → HTTP status code
    ↓
ApiResponse<T>                     ← { success, data, error, meta }
```

---

## Layer chi tiết

### API Layer — `src/DanGian.Api/`

| File | Vai trò |
|------|---------|
| `Controllers/BaseApiController.cs` | Base class: expose `Sender`, `CurrentUserId`, `HandleResult()` |
| `Middleware/GlobalExceptionMiddleware.cs` | Bắt exception → map sang HTTP 4xx/5xx + `ApiResponse<T>` |
| `Hubs/GameHub.cs` | SignalR hub cho real-time game |
| `Program.cs` | Middleware pipeline + DI wiring |

**Middleware order trong Program.cs:**
```
GlobalExceptionMiddleware
→ Serilog request logging
→ CORS
→ HTTPS redirection
→ Authentication (JWT)
→ Authorization
→ MapControllers
→ MapHub<GameHub>("/hubs/game")
```

**BaseApiController pattern:**
```csharp
protected IActionResult HandleResult<T>(Result<T> result) =>
    result.IsSuccess
        ? Ok(ApiResponse<T>.Ok(result.Value))
        : BadRequest(ApiResponse<T>.Fail(result.Error.Code, result.Error.Message));
```

---

### Application Layer — `src/DanGian.Application/`

#### Abstractions

| Interface | Mô tả |
|-----------|-------|
| `ICommand<TResponse>` | Marker interface cho write operations |
| `IQuery<TResponse>` | Marker interface cho read operations |
| `ICommandHandler<TCommand, TResponse>` | Handler cho command |
| `IQueryHandler<TQuery, TResponse>` | Handler cho query |
| `IUnitOfWork` | Commit DB + publish domain events |
| `IJwtTokenGenerator` | Generate JWT token |

#### Pipeline Behaviors (chạy theo thứ tự)

1. **LoggingBehavior** — log `Handling {RequestName}` / `Handled {RequestName}`
2. **ValidationBehavior** — chạy tất cả `IValidator<TRequest>`, throw `ValidationException` nếu fail

#### Cấu trúc Feature

```
Features/
  {Context}/
    Commands/
      {FeatureName}/
        {Name}Command.cs          ← record implements ICommand<TResponse>
        {Name}CommandHandler.cs   ← internal sealed, implements ICommandHandler
        {Name}CommandValidator.cs ← AbstractValidator<TCommand>
        {Name}Response.cs         ← sealed record (DTO trả về)
    Queries/
      {FeatureName}/
        {Name}Query.cs
        {Name}QueryHandler.cs
        {Name}Response.cs
```

#### Bounded Contexts hiện có

| Context | Commands | Queries |
|---------|----------|---------|
| Identity | `LoginCommand` | `GetProfileQuery` |
| Game | `CreateSessionCommand` | *(chưa có)* |
| Mission | *(chưa có)* | *(chưa có)* |
| Leaderboard | *(chưa có)* | *(chưa có)* |

#### DI Registration — `DependencyInjection.cs`

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
services.AddValidatorsFromAssembly(assembly);
```

---

### Domain Layer — `src/DanGian.Domain/`

#### Result Pattern

```csharp
// Tạo thành công
Result.Success(new LoginResponse(...))
Result<T>.Success(value)

// Tạo lỗi
Result.Failure(Error.NotFound("User", id))
Result<T>.Failure(error)
```

#### Error factory

```csharp
Error.NotFound(string resource, object id)    // → 404
Error.Conflict(string code, string message)   // → 409
Error.Validation(string code, string message) // → 422
Error.None                                    // sentinel cho success
```

#### Aggregates

| Aggregate | File |
|-----------|------|
| `User` | `Aggregates/User.cs` |
| `GameSession` | `Aggregates/GameSession.cs` |
| `Room` | `Aggregates/Room.cs` |
| `MissionDefinition` | `Aggregates/MissionDefinition.cs` |
| `UserMissionProgress` | `Aggregates/UserMissionProgress.cs` |
| `Season` | `Aggregates/Season.cs` |

**AggregateRoot** chứa `DomainEvents` list — UnitOfWork publish sau khi SaveChanges.

#### Repository Interfaces

| Interface | Location |
|-----------|----------|
| `IUserRepository` | `IRepositories/IUserRepository.cs` |
| `IGameSessionRepository` | `IRepositories/IGameSessionRepository.cs` |
| `IRoomRepository` | `IRepositories/IRoomRepository.cs` |
| `IMissionRepository` | `IRepositories/IMissionRepository.cs` |
| `ILeaderboardRepository` | `IRepositories/ILeaderboardRepository.cs` |

---

### Infrastructure Layer — `src/DanGian.Infrastructure/`

#### Persistence

| File | Vai trò |
|------|---------|
| `Persistence/ApplicationDbContext.cs` | EF Core DbContext |
| `Persistence/UnitOfWork.cs` | `SaveChangesAsync()` + publish domain events |
| `Persistence/Repositories/BaseRepository<T>` | `GetByIdAsync`, `AddAsync`, `Update` |
| `Persistence/Repositories/UserRepository.cs` | implements `IUserRepository` |
| `Persistence/Repositories/GameSessionRepository.cs` | implements `IGameSessionRepository` |

#### UnitOfWork flow

```
SaveChangesAsync()
  → _context.SaveChangesAsync()      ← commit DB
  → PublishDomainEventsAsync()        ← lấy events từ AggregateRoot, clear, publish qua MediatR IPublisher
```

#### DI Registration — `DependencyInjection.cs`

```csharp
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IGameSessionRepository, GameSessionRepository>();
// ... các repository khác
```

---

## Response format chuẩn

```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "meta": {
    "timestamp": "2026-04-27T...",
    "version": "1.0"
  }
}
```

Lỗi:
```json
{
  "success": false,
  "data": null,
  "error": { "code": "User.NotFound", "message": "User with id '...' was not found." },
  "meta": { ... }
}
```

---

## Exception → HTTP status mapping

| Exception | HTTP |
|-----------|------|
| `NotFoundException` | 404 |
| `ValidationException` | 422 |
| `DomainException` | 400 |
| `UnauthorizedAccessException` | 401 |
| Unhandled | 500 |

---

## JWT Strategy

```
Access Token: JWT Bearer
Claims: sub (userId), role
Validate tại Authentication middleware của ASP.NET Core
CurrentUserId đọc từ ClaimTypes.NameIdentifier hoặc "sub" claim
```

---

## Real-time (SignalR)

Hub: `src/DanGian.Api/Hubs/GameHub.cs`
Endpoint: `/hubs/game`

---

## Thêm feature mới — CQRS checklist

```
1. Domain:        tạo/cập nhật Entity, thêm method vào Aggregate nếu cần
2. Application:   tạo Command/Query + Handler + Validator + Response record
                  → src/DanGian.Application/Features/{Context}/{Commands|Queries}/{Name}/
3. Infrastructure: implement Repository method nếu cần
4. Api:           thêm endpoint vào Controller (kế thừa BaseApiController)
                  → return HandleResult(await Sender.Send(new XxxCommand(...)));
```

---

## Conventions bắt buộc

```csharp
// Handler: internal sealed class
internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>

// Command/Query/Response: sealed record
public sealed record LoginCommand(string ZaloId, string DisplayName) : ICommand<LoginResponse>;
public sealed record LoginResponse(Guid UserId, string AccessToken);

// Repository method: async + CancellationToken
Task<User?> GetByZaloIdAsync(string zaloId, CancellationToken ct = default);

// KHÔNG dùng .Result hoặc .Wait() — deadlock risk
// KHÔNG expose Entity ra ngoài Controller — dùng Response record
// KHÔNG throw exception từ Handler — dùng Result.Failure(error)
```
