using DanGian.Domain.Common;

namespace DanGian.Domain.Leaderboard;

public sealed class SeasonRanking : Entity
{
    private SeasonRanking() { }

    private SeasonRanking(Guid id, Guid seasonId, Guid userId, int rank, int points) : base(id)
    {
        SeasonId = seasonId;
        UserId = userId;
        Rank = rank;
        Points = points;
    }

    public Guid SeasonId { get; private set; }
    public Guid UserId { get; private set; }
    public int Rank { get; private set; }
    public int Points { get; private set; }

    public static SeasonRanking Create(Guid seasonId, Guid userId, int rank, int points) =>
        new(Guid.NewGuid(), seasonId, userId, rank, points);
}
