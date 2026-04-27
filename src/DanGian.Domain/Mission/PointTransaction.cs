using DanGian.Domain.Common;

namespace DanGian.Domain.Mission;

public sealed class PointTransaction : Entity
{
    private PointTransaction() { }

    private PointTransaction(Guid id, Guid userId, int delta, string reason, Guid? referenceId)
        : base(id)
    {
        UserId = userId;
        Delta = delta;
        Reason = reason;
        ReferenceId = referenceId;
    }

    public Guid UserId { get; private set; }
    public int Delta { get; private set; }
    public string Reason { get; private set; } = default!;
    public Guid? ReferenceId { get; private set; }

    public static PointTransaction Create(
        Guid userId,
        int delta,
        string reason,
        Guid? referenceId = null)
    {
        if (delta == 0)
            throw new ArgumentException("Point delta cannot be zero.", nameof(delta));

        return new PointTransaction(Guid.NewGuid(), userId, delta, reason, referenceId);
    }
}
