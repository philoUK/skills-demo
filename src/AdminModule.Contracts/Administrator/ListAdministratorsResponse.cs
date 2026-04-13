namespace AdminModule.Contracts.Administrator;

public record AdministratorResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Status
);

public record ListAdministratorsResponse(IReadOnlyList<AdministratorResponse> Administrators);
