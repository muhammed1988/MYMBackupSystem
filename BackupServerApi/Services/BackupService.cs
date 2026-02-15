using BackupServerApi.Data;
using BackupServerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackupServerApi.Services;

public class BackupService
{
    private readonly BackupDbContext _db;

    public BackupService(BackupDbContext db)
    {
        _db = db;
    }

    public async Task<List<BackupJob>> GetRecentAsync(CancellationToken ct = default)
    {
        return await _db.BackupJobs
            .AsNoTracking()
            .OrderByDescending(j => j.Timestamp)
            .Take(200)
            .ToListAsync(ct);
    }
}