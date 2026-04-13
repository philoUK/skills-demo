using System.Net.Http.Json;
using AdminModule.Administrator.Data;
using AdminModule.Contracts.Administrator;
using Xunit;

namespace AdminModule.IntegrationTests;

public class AdministratorsListTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public AdministratorsListTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ClearAdministratorsAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Returns_administrators_ordered_by_email()
    {
        await _factory.SeedAsync(db =>
        {
            db.Administrators.AddRange(
                MakeEntity("zzz@example.com", "Zed", "Last", "active"),
                MakeEntity("aaa@example.com", "Alice", "First", "active"),
                MakeEntity("mmm@example.com", "Mike", "Middle", "inactive")
            );
        });

        var response = await _client.GetFromJsonAsync<ListAdministratorsResponse>(
            "/admin/administrators"
        );

        Assert.NotNull(response);
        Assert.Equal(3, response.Administrators.Count);
        Assert.Equal("aaa@example.com", response.Administrators[0].Email);
        Assert.Equal("mmm@example.com", response.Administrators[1].Email);
        Assert.Equal("zzz@example.com", response.Administrators[2].Email);
    }

    [Fact]
    public async Task Search_filters_by_partial_email_match_when_3_or_more_chars()
    {
        await _factory.SeedAsync(db =>
        {
            db.Administrators.AddRange(
                MakeEntity("alice@company.com", "Alice", "Smith", "active"),
                MakeEntity("bob@company.com", "Bob", "Jones", "pending"),
                MakeEntity("charlie@other.org", "Charlie", "Brown", "active")
            );
        });

        var response = await _client.GetFromJsonAsync<ListAdministratorsResponse>(
            "/admin/administrators?search=company"
        );

        Assert.NotNull(response);
        Assert.Equal(2, response.Administrators.Count);
        Assert.All(response.Administrators, a => Assert.Contains("company", a.Email));
    }

    [Fact]
    public async Task Search_is_ignored_when_fewer_than_3_chars()
    {
        await _factory.SeedAsync(db =>
        {
            db.Administrators.AddRange(
                MakeEntity("alpha@example.com", "Alpha", "One", "active"),
                MakeEntity("beta@example.com", "Beta", "Two", "inactive")
            );
        });

        var response = await _client.GetFromJsonAsync<ListAdministratorsResponse>(
            "/admin/administrators?search=al"
        );

        Assert.NotNull(response);
        Assert.Equal(2, response.Administrators.Count);
    }

    private static AdministratorEntity MakeEntity(
        string email,
        string firstName,
        string lastName,
        string status
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
}
