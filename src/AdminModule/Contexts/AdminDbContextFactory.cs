using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdminModule.Contexts;

internal class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__admindb")
            ?? "Host=localhost;Database=admindb;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AdminDbContext(options);
    }
}
