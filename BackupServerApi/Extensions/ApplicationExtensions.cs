using Microsoft.AspNetCore.Builder;

namespace BackupServerApi.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Static files, routing, auth, controllers
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}