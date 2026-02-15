using BackupServerApi.Models;

namespace BackupServerApi.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the base URL configured for the API client
    /// </summary>
    public string BaseUrl => _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    public async Task<List<BackupJobDto>> GetAllBackupsAsync(CancellationToken ct = default)
    {
        try
        {
            var backups = await _httpClient.GetFromJsonAsync<List<BackupJobDto>>("/api/backups", ct);
            return backups ?? new List<BackupJobDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch backups from API. Check BaseAddress and API key configuration.");
            return new List<BackupJobDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching backups");
            return new List<BackupJobDto>();
        }
    }

    public async Task<bool> DeleteBackupAsync(int backupId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/backups/{backupId}", ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted backup ID {BackupId}", backupId);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Failed to delete backup ID {BackupId}. Status: {Status}, Body: {Body}",
                backupId, response.StatusCode, errorBody);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while deleting backup ID {BackupId}", backupId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting backup ID {BackupId}", backupId);
            return false;
        }
    }

    /// <summary>
    /// Download backup file with progress reporting
    /// </summary>
    public async Task<DownloadResult?> DownloadAsync(
        int backupId,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/backups/download/{backupId}");
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Download failed for backup ID {Id}. Status: {Status}", backupId, response.StatusCode);
                return null;
            }

            // Get filename from Content-Disposition header
            string fileName = $"backup-{backupId}.zip";
            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/zip";
            var totalBytes = response.Content.Headers.ContentLength ?? 0;

            // Read with progress reporting
            using var contentStream = await response.Content.ReadAsStreamAsync(ct);
            using var memoryStream = new MemoryStream();

            var buffer = new byte[81920]; // 80KB buffer
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
            {
                await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                totalRead += bytesRead;

                if (progress != null && totalBytes > 0)
                {
                    var percentage = (int)((totalRead * 100) / totalBytes);
                    progress.Report(new DownloadProgress
                    {
                        BytesRead = totalRead,
                        TotalBytes = totalBytes,
                        PercentComplete = percentage
                    });
                }
            }

            return new DownloadResult
            {
                Data = memoryStream.ToArray(),
                FileName = fileName,
                ContentType = contentType
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Download canceled for backup ID {Id}", backupId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download backup ID {Id}", backupId);
            return null;
        }
    }

    public class DownloadResult
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/zip";
    }

    public class DownloadProgress
    {
        public long BytesRead { get; set; }
        public long TotalBytes { get; set; }
        public int PercentComplete { get; set; }
    }
}