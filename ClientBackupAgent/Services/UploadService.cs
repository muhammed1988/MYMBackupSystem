using System.Net.Http.Headers;
using ClientBackupAgent.Configuration;
using Microsoft.Extensions.Options;

namespace ClientBackupAgent.Services;

public class UploadService
{
    private readonly HttpClient _httpClient;
    private readonly ClientOptions _clientOptions;
    private readonly ILogger<UploadService> _logger;

    public UploadService(
        HttpClient httpClient,
        IOptions<ClientOptions> clientOptions,
        ILogger<UploadService> logger)
    {
        _httpClient = httpClient;
        _clientOptions = clientOptions.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_clientOptions.ServerBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _clientOptions.ApiKey);
    }

    public async Task UploadBackupAsync(string dbName, string zipPath, CancellationToken ct)
    {
        var fileInfo = new FileInfo(zipPath);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(_clientOptions.ClientId.ToString()), "clientId");
        content.Add(new StringContent(dbName), "databaseName");
        content.Add(new StringContent(DateTime.UtcNow.ToString("O")), "timestamp");

        var fileContent = new StreamContent(File.OpenRead(zipPath));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(fileContent, "file", fileInfo.Name);

        var response = await _httpClient.PostAsync("/api/backups/upload", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to upload backup {File}. Status: {Status}. Body: {Body}",
                zipPath, response.StatusCode, body);
            throw new InvalidOperationException("Upload failed");
        }

        _logger.LogInformation("Uploaded backup {File} successfully", zipPath);
    }
}
