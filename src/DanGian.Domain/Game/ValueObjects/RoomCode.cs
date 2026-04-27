using DanGian.Domain.Common;

namespace DanGian.Domain.Game.ValueObjects;

public sealed class RoomCode : ValueObject
{
    public string Value { get; }

    private RoomCode(string value) => Value = value;

    public static RoomCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 6)
            throw new ArgumentException("Room code must be exactly 6 characters.", nameof(value));

        return new RoomCode(value.ToUpperInvariant());
    }

    public static RoomCode Generate()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new string(Enumerable.Range(0, 6)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
        return new RoomCode(code);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
