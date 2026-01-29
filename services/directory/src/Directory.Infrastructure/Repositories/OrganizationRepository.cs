using Directory.Application.Interfaces;
using Directory.Domain.Entities;
using Directory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Directory.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly DirectoryDbContext _context;

    public OrganizationRepository(DirectoryDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.Slug == Domain.ValueObjects.Slug.FromExisting(slug), cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .AnyAsync(o => o.Slug == Domain.ValueObjects.Slug.FromExisting(slug), cancellationToken);
    }

    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
