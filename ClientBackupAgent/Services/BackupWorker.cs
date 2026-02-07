using ClientBackupAgent.Configuration;
using Microsoft.Extensions.Options;

namespace ClientBackupAgent.Services;

public class BackupWorker : BackgroundService
{
    private readonly ILogger<BackupWorker> _logger;
    private readonly SqlBackupService _sqlBackupService;
    private readonly CompressionService _compressionService;
    private readonly UploadService _uploadService;
    private readonly BackupScheduleOptions _backupOptions;

    public BackupWorker(
        ILogger<BackupWorker> logger,
        SqlBackupService sqlBackupService,
        CompressionService compressionService,
        UploadService uploadService,
        IOptions<BackupScheduleOptions> backupOptions)
    {
        _logger = logger;
        _sqlBackupService = sqlBackupService;
        _compressionService = compressionService;
        _uploadService = uploadService;
        _backupOptions = backupOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_backupOptions.LocalTempPath);

        var interval = TimeSpan.FromMinutes(_backupOptions.IntervalMinutes);
        _logger.LogInformation("Backup worker started. Interval: {Interval}", interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunBackupCycle(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during backup cycle");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private async Task RunBackupCycle(CancellationToken ct)
    {
        _logger.LogInformation("Starting backup cycle at {Time}", DateTimeOffset.Now);

        var databases = await _sqlBackupService.GetUserDatabasesAsync(ct);

        foreach (var dbName in databases)
        {
            ct.ThrowIfCancellationRequested();

            _logger.LogInformation("Backing up database {DbName}", dbName);

            var bakPath = await _sqlBackupService.BackupDatabaseAsync(dbName, _backupOptions.LocalTempPath, ct);
            var zipPath = await _compressionService.CompressAsync(bakPath, ct);

            await _uploadService.UploadBackupAsync(dbName, zipPath, ct);

            File.Delete(bakPath);
            File.Delete(zipPath);
        }

        _logger.LogInformation("Backup cycle finished at {Time}", DateTimeOffset.Now);
    }
}
