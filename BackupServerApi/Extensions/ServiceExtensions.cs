using BackupServerApi.Data;
using BackupServerApi.Services;
using BackupShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackupServerApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Framework & API
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Persistence
        services.AddDbContext<BackupDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("Default"));
        });

        // Application services
        services.AddScoped<BackupService>();

        // Options (strongly typed)
        services.Configure<BackupStorageOptions>(configuration.GetSection("BackupStorage"));
        services.Configure<ApiKeyOptions>(configuration.GetSection("ApiKeys"));

        return services;
    }
}