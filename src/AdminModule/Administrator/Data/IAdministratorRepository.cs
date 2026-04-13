namespace AdminModule.Administrator.Data;

using DomainAdministrator = Domain.Administrator;

internal interface IAdministratorRepository
{
    Task<IReadOnlyList<DomainAdministrator>> ListAsync(
        string? search,
        CancellationToken ct = default
    );
}
