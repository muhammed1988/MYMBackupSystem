using BackupServerApi.Data;
using BackupServerApi.Models;
using BackupShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BackupServerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupsController : ControllerBase
{
    private readonly BackupDbContext _db;
    private readonly BackupStorageOptions _storageOptions;
    private readonly ApiKeyOptions _apiKeyOptions;
    private readonly ILogger<BackupsController> _logger;

    public BackupsController(
        BackupDbContext db,
        IOptions<BackupStorageOptions> storageOptions,
        IOptions<ApiKeyOptions> apiKeyOptions,
        ILogger<BackupsController> logger)
    {
        _db = db;
        _storageOptions = storageOptions.Value;
        _apiKeyOptions = apiKeyOptions.Value;
        _logger = logger;
    }

    private bool IsAuthorized()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var key))
            return false;

        return string.Equals(key.ToString(), _apiKeyOptions.ClientUploadKey, StringComparison.Ordinal);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<IActionResult> Upload(
        [FromForm] Guid clientId,
        [FromForm] string databaseName,
        [FromForm] DateTime timestamp,
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        if (!IsAuthorized())
        {
            return Unauthorized("Invalid API key");
        }

        if (file.Length == 0)
        {
            return BadRequest("Empty file");
        }

        var root = _storageOptions.RootPath;
        var date = timestamp.ToUniversalTime();
        var folder = Path.Combine(
            root,
            clientId.ToString(),
            databaseName,
            date.ToString("yyyy"),
            date.ToString("MM"),
            date.ToString("dd"));

        Directory.CreateDirectory(folder);

        var fileName = $"{databaseName}_{date:yyyyMMdd_HHmmss}.zip";
        var fullPath = Path.Combine(folder, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var job = new BackupJob
        {
            ClientId = clientId,
            DatabaseName = databaseName,
            Timestamp = date,
            FilePath = fullPath,
            FileSizeBytes = file.Length
        };

        _db.BackupJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        var response = new BackupUploadRequest
        {
            ClientId = clientId,
            DatabaseName = databaseName,
            Timestamp = date,
            FileSizeBytes = file.Length,
            FilePath = fullPath
        };

        _logger.LogInformation("Stored backup for {Client} {Db} at {Path}",
            clientId, databaseName, fullPath);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BackupJob>>> GetAll(CancellationToken ct)
    {
        var jobs = await _db.BackupJobs
            .OrderByDescending(j => j.Timestamp)
            .Take(200)
            .ToListAsync(ct);

        return Ok(jobs);
    }
}
