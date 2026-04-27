using DanGian.Application.Abstractions;
using DanGian.Domain.Common;
using DanGian.Domain.Events;
using MediatR;

namespace DanGian.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public UnitOfWork(ApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await _context.SaveChangesAsync(ct);
        await PublishDomainEventsAsync(ct);
        return result;
    }

    private async Task PublishDomainEventsAsync(CancellationToken ct)
    {
        var aggregates = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        foreach (var domainEvent in events)
            await _publisher.Publish((IDomainEvent)domainEvent, ct);
    }
}
