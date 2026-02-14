using BackupServerApi.Data;
using BackupServerApi.Models;
using BackupShared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BackupDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.Configure<BackupStorageOptions>(
    builder.Configuration.GetSection("BackupStorage"));

builder.Services.Configure<ApiKeyOptions>(
    builder.Configuration.GetSection("ApiKeys"));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public class BackupStorageOptions
{
    public string RootPath { get; set; } = "/var/backups/mymbackup";
}

public class ApiKeyOptions
{
    public string ClientUploadKey { get; set; } = "YOUR-CLIENT-API-KEY";
}
