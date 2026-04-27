using DanGian.Domain.Common;

namespace DanGian.Domain.Leaderboard;

public sealed class SeasonRanking : Entity
{
    private SeasonRanking() { }

    public Guid SeasonId { get; private set; }
    public Guid UserId { get; private set; }
    public int Rank { get; private set; }
    public int Points { get; private set; }
}
