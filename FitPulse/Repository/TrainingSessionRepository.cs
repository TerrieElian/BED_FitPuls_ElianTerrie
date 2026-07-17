namespace FitPulse.Repository;

public interface ITrainingSessionRepository
{
    Task<List<TrainingSession>> GetAllForMemberAsync(int memberId);
    Task<TrainingSession?> GetByIdAsync(int id);
    Task AddAsync(TrainingSession session);
    Task UpdateAsync(TrainingSession session);
}

public class TrainingSessionRepository : ITrainingSessionRepository
{
    private readonly FitPulseDbContext _context;

    public TrainingSessionRepository(FitPulseDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrainingSession>> GetAllForMemberAsync(int memberId)
    {
        return await _context.TrainingSessions
            .Include(s => s.Device)
            .Include(s => s.Payment)
            .Where(s => s.MemberId == memberId)
            .ToListAsync();
    }

    public async Task<TrainingSession?> GetByIdAsync(int id)
    {
        return await _context.TrainingSessions
            .Include(s => s.Device)
            .Include(s => s.Member)
            .Include(s => s.Payment)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(TrainingSession session)
    {
        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TrainingSession session)
    {
        _context.Entry(session).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}