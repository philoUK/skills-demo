using AdminModule.Administrator.Data;
using AdminModule.Administrator.Endpoints;
using AdminModule.Keycloak;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AdminModule.Contexts;

internal class DatabaseMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseMigrationService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var keycloakClient = scope.ServiceProvider.GetRequiredService<IKeycloakAdminClient>();
        var frontendOptions = scope.ServiceProvider.GetRequiredService<IOptions<FrontendOptions>>();

        _logger.LogInformation("Applying admin database migrations");
        await db.Database.MigrateAsync(cancellationToken);
        await SeedAsync(db, keycloakClient, frontendOptions.Value, cancellationToken);
    }

    private async Task SeedAsync(
        AdminDbContext db,
        IKeycloakAdminClient keycloakClient,
        FrontendOptions frontendOptions,
        CancellationToken ct
    )
    {
        if (await db.Administrators.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding bootstrap administrator");

        const string email = "pbennett@neworbit.co.uk";
        const string firstName = "Phil";
        const string lastName = "Bennett";

        var keycloakUserId = await keycloakClient.CreateUserAsync(email, firstName, lastName, ct);
        await keycloakClient.AssignRealmRoleAsync(keycloakUserId, "administrator", ct);
        await keycloakClient.SendUpdatePasswordEmailAsync(
            keycloakUserId,
            $"{frontendOptions.AdminUrl}/callback",
            ct
        );

        var now = DateTime.UtcNow;
        db.Administrators.Add(
            new AdministratorEntity
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Status = "active",
                KeycloakUserId = keycloakUserId,
                CreatedAt = now,
                UpdatedAt = now,
            }
        );

        await db.SaveChangesAsync(ct);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
