namespace FitPulse.Models;

public class DiscountCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DiscountType Type { get; set; }

    public DateTime ExpirationDate { get; set; }

    public decimal MinSessionPrice { get; set; }
}