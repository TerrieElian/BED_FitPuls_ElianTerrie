namespace FitPulse.Repository;

public interface IMaintenanceLogRepository
{
    Task<List<MaintenanceLog>> GetAllAsync();
    Task<MaintenanceLog?> GetByIdAsync(int id);
    Task AddAsync(MaintenanceLog log);
    Task UpdateAsync(MaintenanceLog log);
    Task DeleteAsync(int id);
}

public class MaintenanceLogRepository : IMaintenanceLogRepository
{
    private readonly FitPulseDbContext _context;

    public MaintenanceLogRepository(FitPulseDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaintenanceLog>> GetAllAsync()
    {
        return await _context.MaintenanceLogs.Include(m => m.Device).ToListAsync();
    }

    public async Task<MaintenanceLog?> GetByIdAsync(int id)
    {
        return await _context.MaintenanceLogs.Include(m => m.Device).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(MaintenanceLog log)
    {
        _context.MaintenanceLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MaintenanceLog log)
    {
        _context.Entry(log).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var log = await _context.MaintenanceLogs.FindAsync(id);
        if (log != null)
        {
            _context.MaintenanceLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }
}