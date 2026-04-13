using System.Net;
using AdminModule.Administrator.Data;
using Xunit;

namespace AdminModule.IntegrationTests;

public class AdministratorsDeactivateTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public AdministratorsDeactivateTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ClearAdministratorsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Deactivating_an_active_administrator_returns_204()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "active@example.com", "active"));
        });

        var response = await _client.PostAsync($"/admin/administrators/{id}/deactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deactivating_an_active_administrator_persists_inactive_status()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "active@example.com", "active"));
        });

        await _client.PostAsync($"/admin/administrators/{id}/deactivate", null);

        await _factory.AssertAdministratorStatusAsync(id, "inactive");
    }

    [Fact]
    public async Task Deactivating_a_nonexistent_administrator_returns_404()
    {
        var response = await _client.PostAsync($"/admin/administrators/{Guid.NewGuid()}/deactivate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivating_an_already_inactive_administrator_returns_409()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "inactive@example.com", "inactive"));
        });

        var response = await _client.PostAsync($"/admin/administrators/{id}/deactivate", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Deactivating_your_own_account_returns_409()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "self@example.com", "active", keycloakUserId: "test-admin-keycloak-id"));
        });

        var response = await _client.PostAsync($"/admin/administrators/{id}/deactivate", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private static AdministratorEntity MakeEntity(
        Guid id,
        string email,
        string status,
        string? keycloakUserId = null
    ) =>
        new()
        {
            Id = id,
            Email = email,
            FirstName = "Test",
            LastName = "Admin",
            Status = status,
            KeycloakUserId = keycloakUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
}
