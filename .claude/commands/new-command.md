Implement a new CQRS Command for the DanGian Backend project.

The user will provide: feature name, bounded context, and what the command should do.

If the user hasn't provided these, ask:
1. Feature name (e.g. "CreateRoom", "ClaimMission")
2. Bounded context: Identity | Game | Mission | Leaderboard
3. What it does (1-2 sentences)
4. Input fields needed
5. Does it need a new Repository method?

---

When you have the information, generate ALL of the following files:

**File 1:** `src/DanGian.Application/Features/{Context}/Commands/{Name}/{Name}Command.cs`
```csharp
public sealed record {Name}Command(...) : ICommand<{Name}Response>;
```

**File 2:** `src/DanGian.Application/Features/{Context}/Commands/{Name}/{Name}CommandHandler.cs`
- `internal sealed class`, implements `ICommandHandler<{Name}Command, {Name}Response>`
- Inject only: relevant `IRepository`, `IUnitOfWork`, and other needed abstractions
- Return `Result.Success(new {Name}Response(...))` or `Result.Failure(Error.XXX(...))`
- NEVER throw exceptions from the handler body

**File 3:** `src/DanGian.Application/Features/{Context}/Commands/{Name}/{Name}CommandValidator.cs`
- `public sealed class`, extends `AbstractValidator<{Name}Command>`
- Validate all user-facing inputs with FluentValidation rules

**File 4:** `src/DanGian.Application/Features/{Context}/Commands/{Name}/{Name}Response.cs`
```csharp
public sealed record {Name}Response(...);
```

**File 5 (if new repo method needed):** Add method signature to `src/DanGian.Domain/IRepositories/I{Aggregate}Repository.cs` and implement in `src/DanGian.Infrastructure/Persistence/Repositories/{Aggregate}Repository.cs`

**File 6:** Controller endpoint in `src/DanGian.Api/Controllers/{Context}Controller.cs`
- Inherit `BaseApiController`
- Map HTTP verb/route from `docs/docs-backend/API_CONTRACT.md`
- Body: `return HandleResult(await Sender.Send(command, ct));`

**File 7:** `tests/DanGian.UnitTests/Features/{Context}/Commands/{Name}/{Name}CommandHandlerTests.cs`
- Use xUnit + Moq
- Test naming: `Handle_{Scenario}_{ExpectedResult}`
- Cover: happy path, not-found case, validation (test Validator separately)

---

After generating all files, output a checklist:
- [ ] FluentValidation covers all user inputs
- [ ] Handler uses Result.Failure — no throws
- [ ] Unit test covers happy path + failure cases
- [ ] [Authorize] added to controller endpoint if auth required
- [ ] No hardcoded secrets or connection strings
- [ ] async/await + CancellationToken on all DB calls
