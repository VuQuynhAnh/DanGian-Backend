namespace DanGian.Domain.Exceptions;

public sealed class ValidationException : DomainException
{
    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<string> Errors { get; }
}
