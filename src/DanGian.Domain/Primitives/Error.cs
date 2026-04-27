namespace DanGian.Domain.Primitives;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string resource, object id) =>
        new($"{resource}.NotFound", $"{resource} with id '{id}' was not found.");

    public static Error Conflict(string code, string message) =>
        new(code, message);

    public static Error Validation(string code, string message) =>
        new(code, message);
}
