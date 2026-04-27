using DanGian.Domain.Common;

namespace DanGian.Domain.Game.ValueObjects;

public sealed class RoomCode : ValueObject
{
    public string Value { get; }

    private RoomCode(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
