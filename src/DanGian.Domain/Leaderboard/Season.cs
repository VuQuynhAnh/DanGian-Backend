using DanGian.Domain.Common;

namespace DanGian.Domain.Leaderboard;

public sealed class Season : AggregateRoot
{
    private Season() { }

    public int SeasonNum { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool IsActive { get; private set; }
}
