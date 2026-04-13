using AdminModule.Contexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Xunit;

namespace AdminModule.IntegrationTests;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("admindb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the DbContext so it points at the test container
            services.RemoveAll<DbContextOptions<AdminDbContext>>();
            services.AddDbContext<AdminDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString())
            );

            // Replace the migration hosted service with EnsureCreated for tests
            var migrationDescriptor = services.FirstOrDefault(d =>
                d.ImplementationType == typeof(DatabaseMigrationService)
            );
            if (migrationDescriptor != null)
                services.Remove(migrationDescriptor);

            services.AddHostedService<TestDatabaseSetupService>();

            // Override authentication to always grant administrator role
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });

        builder.UseEnvironment("Test");
    }

    internal async Task ClearAdministratorsAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        db.Administrators.RemoveRange(db.Administrators);
        await db.SaveChangesAsync();
    }

    internal async Task SeedAsync(Action<AdminDbContext> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }
}

internal class TestDatabaseSetupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public TestDatabaseSetupService(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
