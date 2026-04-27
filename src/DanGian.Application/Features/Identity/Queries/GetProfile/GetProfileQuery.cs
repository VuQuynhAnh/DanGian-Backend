using DanGian.Application.Abstractions.Messaging;

namespace DanGian.Application.Features.Identity.Queries.GetProfile;

public sealed record GetProfileQuery(Guid UserId) : IQuery<GetProfileResponse>;
