using BackupServerApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackupServerApi.Extensions;

public static class HostExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
            logger.LogInformation("Applying database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations.");
            throw;
        }
    }
}