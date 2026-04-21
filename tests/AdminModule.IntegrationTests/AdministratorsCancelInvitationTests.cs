using System.Net;
using AdminModule.Administrator.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdminModule.IntegrationTests;

public class AdministratorsCancelInvitationTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public AdministratorsCancelInvitationTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ClearAdministratorsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Cancelling_a_pending_invitation_returns_204()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "pending@example.com", "pending"));
        });

        var response = await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/admin/administrators/{id}/invitation")
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Cancelling_a_pending_invitation_removes_the_record()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "pending@example.com", "pending"));
        });

        await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/admin/administrators/{id}/invitation")
        );

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminModule.Contexts.AdminDbContext>();
        var entity = await db.Administrators.FindAsync(id);

        Assert.Null(entity);
    }

    [Fact]
    public async Task Cancelling_a_pending_expired_invitation_returns_204()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "expired@example.com", "pending_expired"));
        });

        var response = await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/admin/administrators/{id}/invitation")
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Cancelling_a_pending_expired_invitation_removes_the_record()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "expired@example.com", "pending_expired"));
        });

        await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/admin/administrators/{id}/invitation")
        );

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminModule.Contexts.AdminDbContext>();
        var entity = await db.Administrators.FindAsync(id);

        Assert.Null(entity);
    }

    [Fact]
    public async Task Cancelling_an_active_administrator_returns_409()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "active@example.com", "active"));
        });

        var response = await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/admin/administrators/{id}/invitation")
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Cancelling_a_nonexistent_administrator_returns_404()
    {
        var response = await _client.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/admin/administrators/{Guid.NewGuid()}/invitation"
            )
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static AdministratorEntity MakeEntity(Guid id, string email, string status) =>
        new()
        {
            Id = id,
            Email = email,
            FirstName = "Test",
            LastName = "Admin",
            Status = status,
            InvitationToken = status is "pending" or "pending_expired" ? "some-token" : null,
            InvitationExpiresAt = status is "pending" or "pending_expired"
                ? DateTime.UtcNow.AddHours(status == "pending_expired" ? -1 : 24)
                : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
}
