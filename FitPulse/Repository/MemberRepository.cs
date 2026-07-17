namespace FitPulse.Repository;

public interface IMemberRepository
{
    Task<Member?> GetByAuth0SubjectAsync(string auth0Subject);
    Task AddAsync(Member member);
    Task UpdateAsync(Member member);
}

public class MemberRepository : IMemberRepository
{
    private readonly FitPulseDbContext _context;

    public MemberRepository(FitPulseDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByAuth0SubjectAsync(string auth0Subject)
    {
        return await _context.Users
            .OfType<Member>()
            .FirstOrDefaultAsync(m => m.Auth0Subject == auth0Subject);
    }

    public async Task AddAsync(Member member)
    {
        _context.Users.Add(member);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Member member)
    {
        _context.Entry(member).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}