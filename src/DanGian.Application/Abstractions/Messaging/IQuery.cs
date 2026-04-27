using DanGian.Domain.Primitives;
using MediatR;

namespace DanGian.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
