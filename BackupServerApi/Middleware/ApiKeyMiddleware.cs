using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackupServerApi.Middleware;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly string _expectedKey;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _expectedKey = options?.Value?.ClientUploadKey ?? string.Empty;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only enforce for API endpoints under /api/backups (adjust as needed)
        if (context.Request.Path.StartsWithSegments("/api/backups", StringComparison.OrdinalIgnoreCase))
        {
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var provided) || string.IsNullOrEmpty(provided))
            {
                _logger.LogWarning("Missing API key for {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API key");
                return;
            }

            if (!string.Equals(provided, _expectedKey, StringComparison.Ordinal))
            {
                _logger.LogWarning("Invalid API key for {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }
        }

        await _next(context);
    }
}