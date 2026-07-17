namespace FitPulse.Profiles;

public class MaintenanceLogProfile : Profile
{
    public MaintenanceLogProfile()
    {
        CreateMap<MaintenanceLog, MaintenanceLogDto>();
    }
}