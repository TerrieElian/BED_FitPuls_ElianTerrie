namespace FitPulse.DTO;

public class PricingRequest
{
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public DeviceType DeviceType { get; set; }
    public DateTime SessionStartTime { get; set; }
    public int LoyaltyPoints { get; set; }
    public DiscountCode? DiscountCode { get; set; }
}