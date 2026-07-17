namespace FitPulse.DTO;

public class DeviceDto
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public int InstallationYear { get; set; }
    public bool IsActive { get; set; }
}