using BackupServerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackupServerApi.Data;

public class BackupDbContext : DbContext
{
    public BackupDbContext(DbContextOptions<BackupDbContext> options) : base(options)
    {
    }

    public DbSet<BackupJob> BackupJobs => Set<BackupJob>();
}
