namespace FitPulse.Grpc;

public class TelemetryIngestService : Telemetry.TelemetryBase
{
    private readonly MongoContext _mongoContext;

    public TelemetryIngestService(MongoContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    public override async Task<TelemetrySummaryResponse> RecordReadings(
        IAsyncStreamReader<TelemetryReadingRequest> requestStream,
        ServerCallContext context)
    {
        var processedCount = 0;

        await foreach (var request in requestStream.ReadAllAsync())
        {
            var reading = new TelemetryReading
            {
                SessionId = request.SessionId,
                DeviceId = request.DeviceId,
                Timestamp = DateTime.UtcNow,
                PowerWatts = request.PowerWatts,
                SpeedOrCadence = request.SpeedOrCadence,
                DistanceOrReps = request.DistanceOrReps,
                HeartRate = request.HeartRate,
                MotorTemp = request.MotorTemp
            };

            await _mongoContext.TelemetryReadings.InsertOneAsync(reading);
            processedCount++;
        }

        return new TelemetrySummaryResponse
        {
            ProcessedCount = processedCount,
            StatusMessage = "Telemetrie-stream succesvol verwerkt."
        };
    }

    public override async Task<SensorDiagnosticResponse> ReportDiagnostic(
    SensorDiagnosticRequest request,
    ServerCallContext context)
    {
        var log = new SensorDiagnosticLog
        {
            DeviceId = request.DeviceId,
            SensorType = request.SensorType,
            ErrorCode = request.ErrorCode,
            Severity = request.Severity,
            Timestamp = DateTime.UtcNow,
            RawSensorDataJson = request.RawSensorDataJson
        };

        await _mongoContext.SensorDiagnosticLogs.InsertOneAsync(log);

        return new SensorDiagnosticResponse { Accepted = true };
    }
}