namespace DanGian.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string name, object key)
        : base($"'{name}' ({key}) was not found.") { }
}
