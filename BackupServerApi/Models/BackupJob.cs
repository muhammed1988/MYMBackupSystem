namespace BackupServerApi.Models;

public class BackupJob
{
    public int Id { get; set; }
    public Guid ClientId { get; set; }
    public string DatabaseName { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public string FilePath { get; set; } = default!;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
