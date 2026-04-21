using System.Net;
using AdminModule.Administrator.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AdminModule.IntegrationTests;

public class AdministratorsRegisterTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public AdministratorsRegisterTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    public async Task InitializeAsync()
    {
        await _factory.ClearAdministratorsAsync();
        _factory.KeycloakAdminClient.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Valid_token_redirects_to_complete_page()
    {
        var token = "valid-token-abc";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakePendingEntity(Guid.NewGuid(), "jane@example.com", token));
        });

        var response = await _client.GetAsync($"/admin/register/{token}");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("register/complete", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Valid_token_marks_administrator_as_active()
    {
        var id = Guid.NewGuid();
        var token = "valid-token-abc";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakePendingEntity(id, "jane@example.com", token));
        });

        await _client.GetAsync($"/admin/register/{token}");

        await _factory.AssertAdministratorStatusAsync(id, "active");
    }

    [Fact]
    public async Task Valid_token_clears_the_invitation_token()
    {
        var id = Guid.NewGuid();
        var token = "valid-token-abc";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakePendingEntity(id, "jane@example.com", token));
        });

        await _client.GetAsync($"/admin/register/{token}");

        await _factory.AssertAdministratorTokenClearedAsync(id);
    }

    [Fact]
    public async Task Valid_token_sets_keycloak_user_id()
    {
        var id = Guid.NewGuid();
        var token = "valid-token-abc";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakePendingEntity(id, "jane@example.com", token));
        });

        await _client.GetAsync($"/admin/register/{token}");

        await _factory.AssertAdministratorKeycloakUserIdAsync(id, "keycloak-user-1");
    }

    [Fact]
    public async Task Valid_token_creates_keycloak_user_with_correct_email()
    {
        var token = "valid-token-abc";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakePendingEntity(Guid.NewGuid(), "jane@example.com", token));
        });

        await _client.GetAsync($"/admin/register/{token}");

        Assert.Contains("jane@example.com", _factory.KeycloakAdminClient.CreatedUserEmails);
    }

    [Fact]
    public async Task Valid_token_assigns_administrator_realm_role()
    {
        var token = "valid-token-abc";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakePendingEntity(Guid.NewGuid(), "jane@example.com", token));
        });

        await _client.GetAsync($"/admin/register/{token}");

        Assert.Contains("keycloak-user-1:administrator", _factory.KeycloakAdminClient.AssignedRoles);
    }

    [Fact]
    public async Task Expired_token_redirects_to_expired_page()
    {
        var token = "expired-token";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeExpiredEntity(Guid.NewGuid(), "expired@example.com", token));
        });

        var response = await _client.GetAsync($"/admin/register/{token}");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("register/expired", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Expired_token_marks_administrator_as_pending_expired()
    {
        var id = Guid.NewGuid();
        var token = "expired-token";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeExpiredEntity(id, "expired@example.com", token));
        });

        await _client.GetAsync($"/admin/register/{token}");

        await _factory.AssertAdministratorStatusAsync(id, "pending_expired");
    }

    [Fact]
    public async Task Unknown_token_redirects_to_not_found_page()
    {
        var response = await _client.GetAsync("/admin/register/no-such-token");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("register/not-found", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Already_used_token_redirects_to_already_used_page()
    {
        var token = "already-used-token";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeActiveEntity(Guid.NewGuid(), "active@example.com", token));
        });

        var response = await _client.GetAsync($"/admin/register/{token}");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("register/already-used", response.Headers.Location?.ToString());
    }

    private static AdministratorEntity MakePendingEntity(Guid id, string email, string token) =>
        new()
        {
            Id = id,
            Email = email,
            FirstName = "Jane",
            LastName = "Doe",
            Status = "pending",
            InvitationToken = token,
            InvitationExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

    private static AdministratorEntity MakeExpiredEntity(Guid id, string email, string token) =>
        new()
        {
            Id = id,
            Email = email,
            FirstName = "Jane",
            LastName = "Doe",
            Status = "pending",
            InvitationToken = token,
            InvitationExpiresAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

    private static AdministratorEntity MakeActiveEntity(Guid id, string email, string token) =>
        new()
        {
            Id = id,
            Email = email,
            FirstName = "Jane",
            LastName = "Doe",
            Status = "active",
            KeycloakUserId = "existing-keycloak-id",
            InvitationToken = token,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
}
