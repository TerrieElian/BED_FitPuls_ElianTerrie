namespace FitPulse.Services;

public interface IDeviceService
{
    Task<List<Device>> GetAllDevicesAsync();
    Task<Device?> GetDeviceByIdAsync(int id);
    Task CreateDeviceAsync(Device device);
    Task UpdateDeviceAsync(Device device);
    Task DeleteDeviceAsync(int id);
}

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IValidator<Device> _validator;

    public DeviceService(IDeviceRepository deviceRepository, IValidator<Device> validator)
    {
        _deviceRepository = deviceRepository;
        _validator = validator;
    }

    public async Task<List<Device>> GetAllDevicesAsync()
    {

        return await _deviceRepository.GetAllAsync();
    }

    public async Task<Device?> GetDeviceByIdAsync(int id)
    {
        return await _deviceRepository.GetByIdAsync(id);
    }

    public async Task CreateDeviceAsync(Device device)
    {
        var validationResult = await _validator.ValidateAsync(device);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await _deviceRepository.AddAsync(device);
    }

    public async Task UpdateDeviceAsync(Device device)
    {
        var validationResult = await _validator.ValidateAsync(device);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await _deviceRepository.UpdateAsync(device);
    }

    public async Task DeleteDeviceAsync(int id)
    {

        await _deviceRepository.DeleteAsync(id);
    }
}