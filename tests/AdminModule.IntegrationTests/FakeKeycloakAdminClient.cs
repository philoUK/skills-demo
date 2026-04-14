using AdminModule.Keycloak;

namespace AdminModule.IntegrationTests;

internal class FakeKeycloakAdminClient : IKeycloakAdminClient
{
    private int _userCounter = 0;

    public List<string> CreatedUserEmails { get; } = [];
    public List<string> AssignedRoles { get; } = [];
    public List<string> SetPasswordEmailsSent { get; } = [];

    public void Clear()
    {
        CreatedUserEmails.Clear();
        AssignedRoles.Clear();
        SetPasswordEmailsSent.Clear();
        _userCounter = 0;
    }

    public Task<string> CreateUserAsync(string email, string firstName, string lastName, CancellationToken ct)
    {
        CreatedUserEmails.Add(email);
        return Task.FromResult($"keycloak-user-{++_userCounter}");
    }

    public Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        AssignedRoles.Add($"{userId}:{roleName}");
        return Task.CompletedTask;
    }

    public Task SendUpdatePasswordEmailAsync(string userId, string redirectUri, CancellationToken ct)
    {
        SetPasswordEmailsSent.Add(userId);
        return Task.CompletedTask;
    }
}
