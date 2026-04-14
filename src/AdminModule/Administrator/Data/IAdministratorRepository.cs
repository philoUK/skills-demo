namespace AdminModule.Administrator.Data;

using DomainAdministrator = Domain.Administrator;

internal interface IAdministratorRepository
{
    Task<IReadOnlyList<DomainAdministrator>> ListAsync(
        string? search,
        CancellationToken ct = default
    );

    Task<DomainAdministrator?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<DomainAdministrator?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task CreateAsync(DomainAdministrator administrator, CancellationToken ct = default);

    Task UpdateAsync(DomainAdministrator administrator, CancellationToken ct = default);
}
