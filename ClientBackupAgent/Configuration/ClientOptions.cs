namespace ClientBackupAgent.Configuration;

public class ClientOptions
{
    public Guid ClientId { get; set; }
    public string ServerBaseUrl { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
}
