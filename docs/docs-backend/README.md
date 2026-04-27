# Dân Gian — Backend Project

## Kiến trúc

**Single API** với **Clean Architecture + Domain Driven Design (DDD)**

```
src/
  DanGian.Domain/          ← Entities, ValueObjects, Interfaces, Domain Events
  DanGian.Application/     ← CQRS (MediatR), Use Cases, Validators
  DanGian.Infrastructure/  ← EF Core, Repositories, JWT, External services
  DanGian.Api/             ← Controllers, SignalR Hubs, Middleware, Program.cs
tests/
  DanGian.UnitTests/       ← Domain + Application tests
  DanGian.IntegrationTests/← API integration tests
```

## Stack

| Layer | Tech |
|-------|------|
| Runtime | C# 12 / .NET 8 (ASP.NET Core) |
| ORM | EF Core 8 + Npgsql (PostgreSQL) |
| CQRS | MediatR 14 |
| Validation | FluentValidation 12 |
| Auth | JWT Bearer |
| Realtime | SignalR |
| Logging | Serilog → stdout |
| Tests | xUnit + Moq + Microsoft.AspNetCore.Mvc.Testing |

## Domain Bounded Contexts

| Context | Aggregates |
|---------|-----------|
| Identity | `User` |
| Game | `GameSession`, `Room` |
| Mission | `MissionDefinition`, `UserMissionProgress` |
| Leaderboard | `Season` |

## Cách thêm feature mới (CQRS pattern)

```
1. Domain: tạo Entity/ValueObject nếu cần, thêm method vào Aggregate
2. Application: tạo Command/Query + Handler + Validator
   src/DanGian.Application/Features/{Context}/{Commands|Queries}/{FeatureName}/
3. Infrastructure: implement Repository method nếu cần
4. Api: thêm endpoint vào Controller (kế thừa BaseApiController)
```

### Ví dụ Command

```csharp
// Application/Features/Identity/Commands/Login/LoginCommand.cs
public record LoginCommand(string ZaloCode) : ICommand<LoginResponse>;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, CancellationToken ct)
    {
        // ...
        return Result.Success(new LoginResponse(...));
    }
}

// Application/Features/Identity/Commands/Login/LoginCommandValidator.cs
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.ZaloCode).NotEmpty();
    }
}
```

## Thứ tự phát triển

```
1. Identity: LoginCommand (JWT issue), GetProfileQuery
2. Game: CreateSessionCommand, MakeMoveCommand, GameHub
3. Mission: GetDailyMissionsQuery, ClaimMissionCommand
4. Leaderboard: GetLeaderboardQuery
```

## Files trong project này

| File | Mục đích |
|------|---------|
| `PROJECT_CONTEXT.md` | Context chung |
| `API_CONTRACT.md` | Interface với Frontend — source of truth |
| `GAME_RULES.md` | Luật Ô Ăn Quan |
| `SYSTEM_DESIGN.md` | Architecture, layers, patterns |
| `DATABASE_SCHEMA.md` | PostgreSQL schema, migrations |
| `FREE_STACK.md` | Supabase, Upstash, Fly.io setup |
| `AI_WORKFLOW.md` | Quy trình làm việc, prompt patterns |
| `SKILL_BACKEND.md` | C# conventions, patterns |

## Migration

```bash
# Từ thư mục root
dotnet ef migrations add InitialCreate \
  --project src/DanGian.Infrastructure \
  --startup-project src/DanGian.Api

dotnet ef database update \
  --project src/DanGian.Infrastructure \
  --startup-project src/DanGian.Api
```

## Environment variables (production)

```
ConnectionStrings__DefaultConnection=Host=...
Jwt__Secret=<min 32 chars>
Jwt__Issuer=dangian-api
Jwt__Audience=dangian-client
```
