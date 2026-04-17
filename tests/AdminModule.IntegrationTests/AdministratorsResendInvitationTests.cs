using System.Net;
using AdminModule.Administrator.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdminModule.IntegrationTests;

public class AdministratorsResendInvitationTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public AdministratorsResendInvitationTests(WebAppFactory factory)
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
    public async Task Resending_a_pending_invitation_returns_204()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "pending@example.com", "pending"));
        });

        var response = await _client.PostAsync(
            $"/admin/administrators/{id}/resend-invitation",
            null
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Resending_a_pending_expired_invitation_returns_204()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "expired@example.com", "pending_expired"));
        });

        var response = await _client.PostAsync(
            $"/admin/administrators/{id}/resend-invitation",
            null
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Resending_generates_a_new_token_and_resets_expiry()
    {
        var id = Guid.NewGuid();
        var originalToken = "original-token";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(
                MakeEntity(id, "pending@example.com", "pending", invitationToken: originalToken)
            );
        });

        await _client.PostAsync($"/admin/administrators/{id}/resend-invitation", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminModule.Contexts.AdminDbContext>();
        var entity = await db.Administrators.FindAsync(id);

        Assert.NotNull(entity!.InvitationToken);
        Assert.NotEqual(originalToken, entity.InvitationToken);
        Assert.NotNull(entity.InvitationExpiresAt);
        Assert.True(entity.InvitationExpiresAt > DateTime.UtcNow.AddHours(23));
    }

    [Fact]
    public async Task Resending_sets_status_back_to_pending()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "expired@example.com", "pending_expired"));
        });

        await _client.PostAsync($"/admin/administrators/{id}/resend-invitation", null);

        await _factory.AssertAdministratorStatusAsync(id, "pending");
    }

    [Fact]
    public async Task Resending_sends_exactly_one_new_email()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "pending@example.com", "pending"));
        });

        await _client.PostAsync($"/admin/administrators/{id}/resend-invitation", null);

        Assert.Single(_factory.EmailService.Sent);
        Assert.Equal("pending@example.com", _factory.EmailService.Sent[0].To);
    }

    [Fact]
    public async Task Resending_sends_email_with_new_token()
    {
        var id = Guid.NewGuid();
        var originalToken = "original-token";
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(
                MakeEntity(id, "pending@example.com", "pending", invitationToken: originalToken)
            );
        });

        await _client.PostAsync($"/admin/administrators/{id}/resend-invitation", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminModule.Contexts.AdminDbContext>();
        var entity = await db.Administrators.FindAsync(id);

        var emailBody = _factory.EmailService.Sent[0].Body;
        Assert.Contains($"/admin/register/{entity!.InvitationToken}", emailBody);
        Assert.DoesNotContain($"/admin/register/{originalToken}", emailBody);
    }

    [Fact]
    public async Task Resending_an_active_administrator_returns_409()
    {
        var id = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Administrators.Add(MakeEntity(id, "active@example.com", "active"));
        });

        var response = await _client.PostAsync(
            $"/admin/administrators/{id}/resend-invitation",
            null
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Resending_a_nonexistent_administrator_returns_404()
    {
        var response = await _client.PostAsync(
            $"/admin/administrators/{Guid.NewGuid()}/resend-invitation",
            null
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static AdministratorEntity MakeEntity(
        Guid id,
        string email,
        string status,
        string? invitationToken = "some-token"
    ) =>
        new()
        {
            Id = id,
            Email = email,
            FirstName = "Test",
            LastName = "Admin",
            Status = status,
            InvitationToken = invitationToken,
            InvitationExpiresAt =
                status == "pending_expired"
                    ? DateTime.UtcNow.AddHours(-1)
                    : DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
}
