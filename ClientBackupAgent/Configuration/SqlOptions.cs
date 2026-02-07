namespace ClientBackupAgent.Configuration;

public class SqlOptions
{
    public string Server { get; set; } = "localhost";
    public bool UseIntegratedSecurity { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
}
