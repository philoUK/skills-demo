namespace AdminModule.Keycloak;

internal interface IKeycloakAdminClient
{
    Task<string> CreateUserAsync(string email, string firstName, string lastName, CancellationToken ct);

    Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken ct);

    Task SendUpdatePasswordEmailAsync(string userId, string redirectUri, CancellationToken ct);
}
