namespace BackupShared;

public class BackupUploadRequest
{
    public Guid ClientId { get; set; }
    public string DatabaseName { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public long FileSizeBytes { get; set; }
    public string? FilePath { get; set; }
}
