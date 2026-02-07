namespace ClientBackupAgent.Configuration;

public class BackupScheduleOptions
{
    public string LocalTempPath { get; set; } = @"C:\\SqlBackups\\Temp";
    public int IntervalMinutes { get; set; } = 60;
}
