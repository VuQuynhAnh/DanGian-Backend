# SKILL — Backend (C# / ASP.NET Core)

> Áp dụng cho mọi coding task trong dự án Dân Gian — Backend.

---

## Stack bắt buộc

- **Runtime:** C# 12 / .NET 8 (ASP.NET Core)
- **Pattern:** Clean Architecture + DDD + CQRS (MediatR)
- **ORM:** EF Core 8 (PostgreSQL provider: Npgsql)
- **Validation:** FluentValidation 12
- **Realtime:** SignalR (không dùng Socket.IO)
- **Testing:** xUnit + Moq
- **Logging:** Serilog → stdout

---

## Architecture (CQRS — MediatR)

```
HTTP Request
→ Controller (kế thừa BaseApiController)
→ Sender.Send(ICommand | IQuery)      ← MediatR dispatch
→ LoggingBehavior → ValidationBehavior ← Pipeline Behaviors
→ CommandHandler | QueryHandler        ← business logic
→ Repository → EF Core → PostgreSQL
→ Result<T>
→ BaseApiController.HandleResult()     ← map → HTTP + ApiResponse<T>
```

**Layer rule:**
- Controller KHÔNG chứa business logic — chỉ gọi `Sender.Send()` và `HandleResult()`
- Handler KHÔNG throw exception — dùng `Result.Failure(error)`
- Repository interface ở Domain, implementation ở Infrastructure (internal)
- Entity KHÔNG expose ra ngoài Controller — dùng Response record

---

## Conventions bắt buộc

### Naming
```csharp
// Classes, Methods, Properties: PascalCase
// Parameters, local variables: camelCase
// Private fields: _camelCase
// Constants: UPPER_SNAKE_CASE
// Interfaces: tiền tố I (IUserRepository)
// Commands: suffix Command (LoginCommand)
// Queries: suffix Query (GetProfileQuery)
// Responses: suffix Response (LoginResponse)
// Validators: suffix Validator (LoginCommandValidator)
// Exceptions: suffix Exception (NotFoundException)
```

### Command / Query / Handler
```csharp
// Command: sealed record implements ICommand<TResponse>
public sealed record LoginCommand(
    string ZaloId,
    string DisplayName,
    string? AvatarUrl) : ICommand<LoginResponse>;

// Query: sealed record implements IQuery<TResponse>
public sealed record GetProfileQuery(Guid UserId) : IQuery<GetProfileResponse>;

// Handler: internal sealed class
internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        // Business logic ở đây
        // KHÔNG throw — dùng Result.Failure
        if (user is null)
            return Result.Failure<LoginResponse>(Error.NotFound("User", request.ZaloId));

        return Result.Success(new LoginResponse(...));
    }
}

// Response: sealed record
public sealed record LoginResponse(
    Guid UserId,
    string DisplayName,
    string AccessToken);
```

### Controller
```csharp
// Kế thừa BaseApiController, KHÔNG inject service trực tiếp
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : BaseApiController
{
    [HttpPost("zalo")]
    public async Task<IActionResult> Login(
        LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.ZaloId, request.DisplayName, request.AvatarUrl);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }
}
```

### Validator
```csharp
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.ZaloId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
    }
}
```

### Repository
```csharp
// Interface ở Domain
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByZaloIdAsync(string zaloId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}

// Implementation ở Infrastructure (internal)
internal sealed class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByZaloIdAsync(string zaloId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.ZaloId == zaloId && u.DeletedAt == null, ct);
}
```

### Result Pattern
```csharp
// Thành công
return Result.Success(new LoginResponse(...));
return Result.Success<LoginResponse>(response);

// Thất bại — KHÔNG throw
return Result.Failure<LoginResponse>(Error.NotFound("User", id));
return Result.Failure<LoginResponse>(Error.Conflict("User.AlreadyExists", "..."));
return Result.Failure<LoginResponse>(Error.Validation("User.InvalidInput", "..."));
```

### Async
```csharp
// LUÔN async/await cho DB và I/O
// LUÔN nhận CancellationToken
// KHÔNG dùng .Result hoặc .Wait() — deadlock risk
// KHÔNG dùng async void — dùng async Task
public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
    await DbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
```

### Không hardcode
```csharp
// LUÔN đọc từ IConfiguration hoặc Environment
var secret = _config["Jwt__Secret"]
    ?? throw new InvalidOperationException("Jwt__Secret not configured");

// KHÔNG BAO GIỜ
var secret = "my-secret-key-hardcoded"; // ❌
```

---

## Cấu trúc file feature mới

```
src/DanGian.Application/Features/{Context}/{Commands|Queries}/{FeatureName}/
  ├── {Name}Command.cs          ← sealed record : ICommand<TResponse>
  ├── {Name}CommandHandler.cs   ← internal sealed class : ICommandHandler<...>
  ├── {Name}CommandValidator.cs ← sealed class : AbstractValidator<TCommand>
  └── {Name}Response.cs         ← sealed record (DTO trả về)
```

Contexts: `Identity`, `Game`, `Mission`, `Leaderboard`

---

## Checklist trước khi merge

- [ ] Unit test pass (xUnit + Moq)
- [ ] Không hardcode credential / connection string
- [ ] FluentValidation đầy đủ cho mọi input từ user
- [ ] Async/await + CancellationToken trên mọi method DB/I/O
- [ ] Handler dùng Result.Failure thay vì throw
- [ ] Logging không chứa sensitive data (token, password)
- [ ] Follow response format `ApiResponse<T>` từ `API_CONTRACT.md`
- [ ] `[Authorize]` attribute trên endpoint cần auth

---

## Test naming convention
```csharp
// [MethodName]_[Scenario]_[ExpectedResult]
public async Task Handle_ValidZaloId_ReturnsLoginResponse() { }
public async Task Handle_EmptyZaloId_ReturnsFailure() { }
public async Task Handle_UserNotFound_ReturnsNotFoundError() { }
public void Validate_MissingZaloId_HasValidationError() { }
```

---

## Security checklist

- [ ] Không hardcode secret / credential / connection string
- [ ] Input validation với FluentValidation tại mọi Command/Query
- [ ] EF Core parameterized query (không raw string concat)
- [ ] JWT claims được validate đầy đủ (issuer, audience, expiry)
- [ ] `[Authorize]` attribute trên mọi endpoint cần auth
- [ ] Sensitive data không log (password, token, zalo_secret)
- [ ] HTTPS-only cho production (HSTS header)

---

## Lỗi phổ biến cần tránh

| Lỗi | Cách đúng |
|-----|-----------|
| `async void` | `async Task` |
| `.Result` / `.Wait()` | `await` |
| Throw từ Handler | `Result.Failure(error)` |
| Inject DbContext trực tiếp vào Handler | Inject Repository + IUnitOfWork |
| N+1 query | `.Include()` hoặc explicit join |
| Thiếu index trên FK | Luôn index FK và WHERE columns |
| Magic string | Constant hoặc enum |
| Return null từ Query Handler | `Result.Failure(Error.NotFound(...))` |
