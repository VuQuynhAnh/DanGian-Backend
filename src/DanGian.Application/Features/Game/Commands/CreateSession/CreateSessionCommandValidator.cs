using FluentValidation;

namespace DanGian.Application.Features.Game.Commands.CreateSession;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Player1Id).NotEmpty();
        RuleFor(x => x.GameType).IsInEnum();
        RuleFor(x => x.Mode).IsInEnum();
        RuleFor(x => x.InitialState).NotEmpty();
    }
}
