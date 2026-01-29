using Directory.Domain.Entities;
using Directory.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Directory.Infrastructure.Persistence;

public class DirectoryDbContext : DbContext
{
    private readonly IPublisher _publisher;

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    public DirectoryDbContext(DbContextOptions<DirectoryDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("directory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryDbContext).Assembly);
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
