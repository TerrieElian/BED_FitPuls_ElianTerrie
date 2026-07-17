namespace FitPulse.Services;

public interface IMatchingService
{
    Task<Device?> FindAvailableDeviceAsync(DeviceType deviceType);
}

public class MatchingService : IMatchingService
{
    private readonly FitPulseDbContext _context;

    public MatchingService(FitPulseDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> FindAvailableDeviceAsync(DeviceType deviceType)
    {
        var busyDeviceIds = _context.TrainingSessions
            .Where(s => s.Status == TrainingSessionStatus.Requested || s.Status == TrainingSessionStatus.InProgress)
            .Select(s => s.DeviceId);

        return await _context.Devices
            .Where(d => d.DeviceType == deviceType && d.IsActive && !busyDeviceIds.Contains(d.Id))
            .OrderBy(d => d.Id)
            .FirstOrDefaultAsync();
    }
}