using System.Net;
using AdminModule.Administrator.Data;
using Xunit;

namespace AdminModule.IntegrationTests;

public class AdministratorsReactivateTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public AdministratorsReactivateTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ClearAdministratorsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Reactivating_an_inactive_administrator_returns_204()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "inactive@example.com", "inactive"));
        });

        var response = await _client.PostAsync($"/admin/administrators/{id}/reactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Reactivating_an_inactive_administrator_persists_active_status()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "inactive@example.com", "inactive"));
        });

        await _client.PostAsync($"/admin/administrators/{id}/reactivate", null);

        await _factory.AssertAdministratorStatusAsync(id, "active");
    }

    [Fact]
    public async Task Reactivating_a_nonexistent_administrator_returns_404()
    {
        var response = await _client.PostAsync($"/admin/administrators/{Guid.NewGuid()}/reactivate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Reactivating_an_already_active_administrator_returns_409()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "active@example.com", "active"));
        });

        var response = await _client.PostAsync($"/admin/administrators/{id}/reactivate", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private static AdministratorEntity MakeEntity(Guid id, string email, string status) =>
        new()
        {
            Id = id,
            Email = email,
            FirstName = "Test",
            LastName = "Admin",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
}
