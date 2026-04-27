using DanGian.Domain.Primitives;
using MediatR;

namespace DanGian.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
