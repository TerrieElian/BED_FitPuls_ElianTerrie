namespace FitPulse.GraphQL;

[ExtendObjectType(typeof(Device))]
public class DeviceExtensions
{
    public async Task<List<SensorDiagnosticLog>> GetSensorDiagnostics(
        [Parent] Device device,
        MongoContext mongoContext)
    {
        return await mongoContext.SensorDiagnosticLogs
            .Find(d => d.DeviceId == device.Id)
            .SortByDescending(d => d.Timestamp)
            .ToListAsync();
    }
}
