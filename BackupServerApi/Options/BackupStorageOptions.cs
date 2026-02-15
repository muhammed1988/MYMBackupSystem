namespace BackupServerApi.Options;

// Keeps configuration in a single place and makes it easy to inject IOptions<T>
public class BackupStorageOptions
{
    public string RootPath { get; set; } = "/var/backups/mymbackup";
}