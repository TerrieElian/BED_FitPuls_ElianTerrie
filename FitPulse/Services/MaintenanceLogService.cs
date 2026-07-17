namespace FitPulse.Services;

public interface IMaintenanceLogService
{
    Task<List<MaintenanceLog>> GetAllMaintenanceLogsAsync();
    Task<MaintenanceLog?> GetMaintenanceLogByIdAsync(int id);
    Task CreateMaintenanceLogAsync(MaintenanceLog log);
    Task UpdateMaintenanceLogAsync(MaintenanceLog log);
    Task DeleteMaintenanceLogAsync(int id);
}

public class MaintenanceLogService : IMaintenanceLogService
{
    private readonly IMaintenanceLogRepository _maintenanceLogRepository;
    private readonly IValidator<MaintenanceLog> _validator;

    public MaintenanceLogService(IMaintenanceLogRepository maintenanceLogRepository, IValidator<MaintenanceLog> validator)
    {
        _maintenanceLogRepository = maintenanceLogRepository;
        _validator = validator;
    }

    public async Task<List<MaintenanceLog>> GetAllMaintenanceLogsAsync()
    {
        return await _maintenanceLogRepository.GetAllAsync();
    }

    public async Task<MaintenanceLog?> GetMaintenanceLogByIdAsync(int id)
    {
        return await _maintenanceLogRepository.GetByIdAsync(id);
    }

    public async Task CreateMaintenanceLogAsync(MaintenanceLog log)
    {
        var validationResult = await _validator.ValidateAsync(log);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        await _maintenanceLogRepository.AddAsync(log);
    }

    public async Task UpdateMaintenanceLogAsync(MaintenanceLog log)
    {
        var validationResult = await _validator.ValidateAsync(log);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        await _maintenanceLogRepository.UpdateAsync(log);
    }

    public async Task DeleteMaintenanceLogAsync(int id)
    {
        await _maintenanceLogRepository.DeleteAsync(id);
    }
}