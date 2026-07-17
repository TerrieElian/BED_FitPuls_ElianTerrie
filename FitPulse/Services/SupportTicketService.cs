namespace FitPulse.Services;

public interface ISupportTicketService
{
    Task<SupportTicket> CreateTicketAsync(int memberId, string subject, string description, TicketPriority priority);
    Task<List<SupportTicket>> GetTicketsForMemberAsync(int memberId);
    Task<List<SupportTicket>> GetAllTicketsAsync();
    Task<SupportTicket?> GetTicketByIdAsync(int id);
    Task<SupportTicket> UpdateStatusAsync(int id, TicketStatus status);
}

public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _ticketRepository;

    public SupportTicketService(ISupportTicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<SupportTicket> CreateTicketAsync(int memberId, string subject, string description, TicketPriority priority)
    {
        var ticket = new SupportTicket
        {
            MemberId = memberId,
            Subject = subject,
            Description = description,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        await _ticketRepository.AddAsync(ticket);
        return ticket;
    }

    public async Task<List<SupportTicket>> GetTicketsForMemberAsync(int memberId)
    {
        return await _ticketRepository.GetAllForMemberAsync(memberId);
    }

    public async Task<List<SupportTicket>> GetAllTicketsAsync()
    {
        return await _ticketRepository.GetAllAsync();
    }

    public async Task<SupportTicket?> GetTicketByIdAsync(int id)
    {
        return await _ticketRepository.GetByIdAsync(id);
    }

    public async Task<SupportTicket> UpdateStatusAsync(int id, TicketStatus status)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket {id} niet gevonden.");
        }

        ticket.Status = status;
        await _ticketRepository.UpdateAsync(ticket);
        return ticket;
    }
}