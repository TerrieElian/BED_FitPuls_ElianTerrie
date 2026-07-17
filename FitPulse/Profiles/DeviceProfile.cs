

namespace FitPulse.Profiles;

public class DeviceProfile : Profile
{
    public DeviceProfile()
    {
        CreateMap<Device, DeviceDto>();
    }
}