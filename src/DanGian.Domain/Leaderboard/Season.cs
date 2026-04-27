using DanGian.Domain.Common;

namespace DanGian.Domain.Leaderboard;

public sealed class Season : AggregateRoot
{
    private readonly List<SeasonRanking> _rankings = [];

    private Season() { }

    private Season(Guid id, int seasonNum, DateOnly startDate, DateOnly endDate) : base(id)
    {
        SeasonNum = seasonNum;
        StartDate = startDate;
        EndDate = endDate;
    }

    public int SeasonNum { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<SeasonRanking> Rankings => _rankings.AsReadOnly();

    public static Season Create(int seasonNum, DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        return new Season(Guid.NewGuid(), seasonNum, startDate, endDate);
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOngoing() =>
        IsActive &&
        DateOnly.FromDateTime(DateTime.UtcNow) >= StartDate &&
        DateOnly.FromDateTime(DateTime.UtcNow) <= EndDate;
}
