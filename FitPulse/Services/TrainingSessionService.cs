namespace FitPulse.Services;

public interface ITrainingSessionService
{
    Task<TrainingSession> RequestSessionAsync(int memberId, DeviceType deviceType);
    Task<TrainingSession> StartSessionAsync(int sessionId);
    Task<TrainingSession> CompleteSessionAsync(int sessionId, int durationMinutes, int caloriesBurned);
    Task<TrainingSession> CancelSessionAsync(int sessionId);
    Task<List<TrainingSession>> GetSessionsForMemberAsync(int memberId);
    Task<TrainingSession?> GetSessionByIdAsync(int id);
}

public class TrainingSessionService : ITrainingSessionService
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IMatchingService _matchingService;

    public TrainingSessionService(ITrainingSessionRepository sessionRepository, IMatchingService matchingService)
    {
        _sessionRepository = sessionRepository;
        _matchingService = matchingService;
    }

    public async Task<TrainingSession> RequestSessionAsync(int memberId, DeviceType deviceType)
    {
        var device = await _matchingService.FindAvailableDeviceAsync(deviceType);
        if (device is null)
        {
            throw new InvalidOperationException($"Geen beschikbaar toestel van type {deviceType}.");
        }

        var session = new TrainingSession
        {
            MemberId = memberId,
            DeviceId = device.Id,
            Status = TrainingSessionStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        await _sessionRepository.AddAsync(session);
        return session;
    }

    public async Task<TrainingSession> StartSessionAsync(int sessionId)
    {
        var session = await GetSessionOrThrowAsync(sessionId);
        if (session.Status != TrainingSessionStatus.Requested)
        {
            throw new InvalidOperationException($"Sessie kan niet gestart worden vanuit status {session.Status}.");
        }

        session.Status = TrainingSessionStatus.InProgress;
        session.StartedAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session);
        return session;
    }

    public async Task<TrainingSession> CompleteSessionAsync(int sessionId, int durationMinutes, int caloriesBurned)
    {
        var session = await GetSessionOrThrowAsync(sessionId);
        if (session.Status != TrainingSessionStatus.InProgress)
        {
            throw new InvalidOperationException($"Sessie kan niet voltooid worden vanuit status {session.Status}.");
        }

        session.Status = TrainingSessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        session.DurationMinutes = durationMinutes;
        session.CaloriesBurned = caloriesBurned;
        await _sessionRepository.UpdateAsync(session);
        return session;
    }

    public async Task<TrainingSession> CancelSessionAsync(int sessionId)
    {
        var session = await GetSessionOrThrowAsync(sessionId);
        if (session.Status is not (TrainingSessionStatus.Requested or TrainingSessionStatus.InProgress))
        {
            throw new InvalidOperationException($"Sessie kan niet geannuleerd worden vanuit status {session.Status}.");
        }

        session.Status = TrainingSessionStatus.Cancelled;
        await _sessionRepository.UpdateAsync(session);
        return session;
    }

    private async Task<TrainingSession> GetSessionOrThrowAsync(int sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session is null)
        {
            throw new KeyNotFoundException($"Sessie {sessionId} niet gevonden.");
        }

        return session;
    }

    // implementatie
    public async Task<List<TrainingSession>> GetSessionsForMemberAsync(int memberId)
    {
        return await _sessionRepository.GetAllForMemberAsync(memberId);
    }

    public async Task<TrainingSession?> GetSessionByIdAsync(int id)
    {
        return await _sessionRepository.GetByIdAsync(id);
    }
}