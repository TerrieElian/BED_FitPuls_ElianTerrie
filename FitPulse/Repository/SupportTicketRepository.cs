namespace FitPulse.Repository;

public interface ISupportTicketRepository
{
    Task<List<SupportTicket>> GetAllAsync();
    Task<List<SupportTicket>> GetAllForMemberAsync(int memberId);
    Task<SupportTicket?> GetByIdAsync(int id);
    Task AddAsync(SupportTicket ticket);
    Task UpdateAsync(SupportTicket ticket);
}

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly FitPulseDbContext _context;

    public SupportTicketRepository(FitPulseDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupportTicket>> GetAllAsync()
    {
        return await _context.SupportTickets.Include(t => t.Member).ToListAsync();
    }

    public async Task<List<SupportTicket>> GetAllForMemberAsync(int memberId)
    {
        return await _context.SupportTickets
            .Where(t => t.MemberId == memberId)
            .ToListAsync();
    }

    public async Task<SupportTicket?> GetByIdAsync(int id)
    {
        return await _context.SupportTickets.Include(t => t.Member).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(SupportTicket ticket)
    {
        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SupportTicket ticket)
    {
        _context.Entry(ticket).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}