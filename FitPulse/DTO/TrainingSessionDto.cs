namespace FitPulse.DTO;

public class TrainingSessionDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int DeviceId { get; set; }
    public string DeviceSerialNumber { get; set; } = string.Empty;
    public TrainingSessionStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationMinutes { get; set; }
    public int? CaloriesBurned { get; set; }
}