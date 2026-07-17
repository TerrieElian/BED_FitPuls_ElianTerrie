namespace FitPulse.Models;

public class SensorDiagnosticLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public int DeviceId { get; set; }
    public string SensorType { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string RawSensorDataJson { get; set; } = string.Empty;
}
