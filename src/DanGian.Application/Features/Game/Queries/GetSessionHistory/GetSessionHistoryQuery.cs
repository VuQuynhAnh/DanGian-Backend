using DanGian.Application.Abstractions.Messaging;
using DanGian.Application.Common;

namespace DanGian.Application.Features.Game.Queries.GetSessionHistory;

public sealed record GetSessionHistoryQuery(
    Guid PlayerId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedList<SessionHistoryItem>>;
