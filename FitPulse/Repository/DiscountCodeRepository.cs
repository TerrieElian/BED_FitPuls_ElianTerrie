namespace FitPulse.Repository;

public interface IDiscountCodeRepository
{
    Task<DiscountCode?> GetByCodeAsync(string code);
}

public class DiscountCodeRepository : IDiscountCodeRepository
{
    private readonly FitPulseDbContext _context;

    public DiscountCodeRepository(FitPulseDbContext context)
    {
        _context = context;
    }

    public async Task<DiscountCode?> GetByCodeAsync(string code)
    {
        return await _context.DiscountCodes.FirstOrDefaultAsync(d => d.Code == code);
    }
}