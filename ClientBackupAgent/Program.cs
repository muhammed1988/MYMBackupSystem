using ClientBackupAgent.Configuration;
using ClientBackupAgent.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ClientOptions>(
    builder.Configuration.GetSection("Client"));
builder.Services.Configure<SqlOptions>(
    builder.Configuration.GetSection("Sql"));
builder.Services.Configure<BackupScheduleOptions>(
    builder.Configuration.GetSection("Backup"));

builder.Services.AddHttpClient<UploadService>();
builder.Services.AddSingleton<SqlBackupService>();
builder.Services.AddSingleton<CompressionService>();
builder.Services.AddHostedService<BackupWorker>();

// For Windows Service later:
builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();
