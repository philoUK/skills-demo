using AdminModule.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AdminModule.Administrator.Data;

using DomainAdministrator = Domain.Administrator;

internal class AdministratorRepository : IAdministratorRepository
{
    private readonly AdminDbContext _db;

    public AdministratorRepository(AdminDbContext db) => _db = db;

    public async Task<IReadOnlyList<DomainAdministrator>> ListAsync(
        string? search,
        CancellationToken ct = default
    )
    {
        var query = _db.Administrators.AsQueryable();

        if (search is { Length: >= 3 })
            query = query.Where(a => a.Email.Contains(search));

        var entities = await query.OrderBy(a => a.Email).ToListAsync(ct);

        return entities.Select(e => e.ToDomain()).ToList();
    }
}
