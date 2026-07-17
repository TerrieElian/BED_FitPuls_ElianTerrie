namespace FitPulse.DTO;

public class MaintenanceLogDto
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string DeviceSerialNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime? NextServiceDue { get; set; }
}