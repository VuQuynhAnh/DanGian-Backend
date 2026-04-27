using DanGian.Domain.Common;

namespace DanGian.Domain.Mission;

public sealed class PointTransaction : Entity
{
    private PointTransaction() { }

    public Guid UserId { get; private set; }
    public int Delta { get; private set; }
    public string Reason { get; private set; } = default!;
    public Guid? ReferenceId { get; private set; }
}
