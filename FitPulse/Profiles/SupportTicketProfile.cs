namespace FitPulse.Profiles;

public class SupportTicketProfile : Profile
{
    public SupportTicketProfile()
    {
        CreateMap<SupportTicket, SupportTicketDto>();
    }
}