namespace FitPulse.Models;

public class MaintenanceLog
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime? NextServiceDue { get; set; }
}
