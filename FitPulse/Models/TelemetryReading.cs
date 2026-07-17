namespace FitPulse.Models;

public class TelemetryReading
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public int SessionId { get; set; }
    public int DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public double PowerWatts { get; set; }
    public double SpeedOrCadence { get; set; }
    public double DistanceOrReps { get; set; }
    public int HeartRate { get; set; }
    public double MotorTemp { get; set; }
}
