using AdminModule.Administrator.Domain;
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

    public async Task<DomainAdministrator?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Administrators.FindAsync([id], ct);
        return entity?.ToDomain();
    }

    public async Task<DomainAdministrator?> GetByEmailAsync(
        string email,
        CancellationToken ct = default
    )
    {
        var entity = await _db.Administrators.FirstOrDefaultAsync(a => a.Email == email, ct);
        return entity?.ToDomain();
    }

    public async Task<DomainAdministrator?> GetByInvitationTokenAsync(
        string token,
        CancellationToken ct = default
    )
    {
        var entity = await _db.Administrators.FirstOrDefaultAsync(
            a => a.InvitationToken == token,
            ct
        );
        return entity?.ToDomain();
    }

    public async Task CreateAsync(DomainAdministrator administrator, CancellationToken ct = default)
    {
        _db.Administrators.Add(AdministratorEntity.FromDomain(administrator));
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DomainAdministrator administrator, CancellationToken ct = default)
    {
        var entity = await _db.Administrators.FindAsync([administrator.Id], ct);
        if (entity is null)
            return;

        entity.Status = AdministratorStatusFactory.ToStorageString(administrator.Status);
        entity.KeycloakUserId = administrator.KeycloakUserId;
        entity.InvitationToken = administrator.InvitationToken?.Value;
        entity.InvitationExpiresAt = administrator.InvitationExpiresAt;
        entity.UpdatedAt = administrator.UpdatedAt;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Administrators.FindAsync([id], ct);
        if (entity is null)
            return;

        _db.Administrators.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
