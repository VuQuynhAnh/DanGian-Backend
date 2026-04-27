using DanGian.Application.Abstractions.Messaging;

namespace DanGian.Application.Features.Mission.Queries.GetDailyMissions;

public sealed record GetDailyMissionsQuery(Guid UserId) : IQuery<IReadOnlyList<MissionProgressItem>>;
