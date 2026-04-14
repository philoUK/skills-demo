using System.Net;
using System.Net.Http.Json;
using AdminModule.Administrator.Data;
using AdminModule.Contracts.Administrator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdminModule.IntegrationTests;

public class AdministratorsInviteTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public AdministratorsInviteTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ClearAdministratorsAsync();
        _factory.EmailService.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Inviting_a_new_administrator_returns_204()
    {
        var response = await _client.PostAsJsonAsync(
            "/admin/administrators/invite",
            new InviteAdministratorRequest("Jane", "Doe", "jane@example.com")
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Inviting_a_new_administrator_creates_a_pending_record()
    {
        await _client.PostAsJsonAsync(
            "/admin/administrators/invite",
            new InviteAdministratorRequest("Jane", "Doe", "jane@example.com")
        );

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminModule.Contexts.AdminDbContext>();
        var entity = db.Administrators.Single(a => a.Email == "jane@example.com");

        Assert.Equal("pending", entity.Status);
        Assert.NotNull(entity.InvitationToken);
        Assert.NotNull(entity.InvitationExpiresAt);
        Assert.True(entity.InvitationExpiresAt > DateTime.UtcNow.AddHours(23));
    }

    [Fact]
    public async Task Inviting_a_new_administrator_sends_exactly_one_email()
    {
        await _client.PostAsJsonAsync(
            "/admin/administrators/invite",
            new InviteAdministratorRequest("Jane", "Doe", "jane@example.com")
        );

        Assert.Single(_factory.EmailService.Sent);
        Assert.Equal("jane@example.com", _factory.EmailService.Sent[0].To);
    }

    [Fact]
    public async Task Invitation_email_contains_registration_link_with_token()
    {
        await _client.PostAsJsonAsync(
            "/admin/administrators/invite",
            new InviteAdministratorRequest("Jane", "Doe", "jane@example.com")
        );

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminModule.Contexts.AdminDbContext>();
        var entity = db.Administrators.Single(a => a.Email == "jane@example.com");

        var emailBody = _factory.EmailService.Sent[0].Body;
        Assert.Contains($"/admin/register/{entity.InvitationToken}", emailBody);
    }

    [Fact]
    public async Task Inviting_a_duplicate_email_returns_409()
    {
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(Guid.NewGuid(), "existing@example.com", "active"));
        });

        var response = await _client.PostAsJsonAsync(
            "/admin/administrators/invite",
            new InviteAdministratorRequest("John", "Smith", "existing@example.com")
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Inviting_a_duplicate_email_with_pending_status_returns_409()
    {
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(Guid.NewGuid(), "pending@example.com", "pending"));
        });

        var response = await _client.PostAsJsonAsync(
            "/admin/administrators/invite",
            new InviteAdministratorRequest("John", "Smith", "pending@example.com")
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Inviting_a_duplicate_email_does_not_send_an_email()
    {
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(Guid.NewGuid(), "existing@example.com", "active"));
        });

        await _client.PostAsJsonAsync(
            "/admin/administrators/invite",
            new InviteAdministratorRequest("John", "Smith", "existing@example.com")
        );

        Assert.Empty(_factory.EmailService.Sent);
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
