using AccessControl.Domain.Entities;
using AccessControl.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Persistence;

public class AccessControlDbContext : DbContext
{
    private readonly IPublisher _publisher;

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

    public AccessControlDbContext(DbContextOptions<AccessControlDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("access_control");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccessControlDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchDomainEvents(domainEvents, cancellationToken);

        return result;
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entities)
            entity.ClearDomainEvents();

        return events;
    }

    private async Task DispatchDomainEvents(List<IDomainEvent> events, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in events)
            await _publisher.Publish(domainEvent, cancellationToken);
    }
}
