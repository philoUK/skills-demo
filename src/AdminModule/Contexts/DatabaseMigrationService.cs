using AdminModule.Administrator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        _logger.LogInformation("Applying admin database migrations");
        await db.Database.MigrateAsync(cancellationToken);
        await SeedAsync(db, cancellationToken);
    }

    private async Task SeedAsync(AdminDbContext db, CancellationToken ct)
    {
        if (await db.Administrators.AnyAsync(ct))
            return;

        _logger.LogInformation("Seeding bootstrap administrator");

        var now = DateTime.UtcNow;
        db.Administrators.Add(
            new AdministratorEntity
            {
                Id = Guid.NewGuid(),
                Email = "pbennett@neworbit.co.uk",
                FirstName = "Phil",
                LastName = "Bennett",
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now,
            }
        );

        await db.SaveChangesAsync(ct);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
