namespace FitPulse.GraphQL;

[ExtendObjectType(typeof(TrainingSession))]
public class TrainingSessionExtensions
{
    public async Task<List<TelemetryReading>> GetTelemetryReadings(
        [Parent] TrainingSession session,
        MongoContext mongoContext)
    {
        return await mongoContext.TelemetryReadings
            .Find(t => t.SessionId == session.Id)
            .ToListAsync();
    }
}