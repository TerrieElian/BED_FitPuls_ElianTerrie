namespace FitPulse.Profiles;

public class TrainingSessionProfile : Profile
{
    public TrainingSessionProfile()
    {
        CreateMap<TrainingSession, TrainingSessionDto>();
    }
}