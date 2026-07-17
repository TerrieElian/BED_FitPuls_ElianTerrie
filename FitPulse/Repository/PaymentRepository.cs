namespace FitPulse.Repository;

public interface IPaymentRepository
{
    Task<Payment?> GetBySessionIdAsync(int sessionId);
    Task AddAsync(Payment payment);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly FitPulseDbContext _context;

    public PaymentRepository(FitPulseDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetBySessionIdAsync(int sessionId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.SessionId == sessionId);
    }

    public async Task AddAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }
}