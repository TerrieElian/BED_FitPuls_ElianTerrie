namespace FitPulse.DTO;

public class PricingResult
{
    public decimal BasePrice { get; set; }
    public decimal PriceAfterMultiplier { get; set; }
    public decimal PriceAfterPeakHourSurcharge { get; set; }
    public decimal LoyaltyDiscount { get; set; }
    public decimal DiscountCodeAmount { get; set; }
    public decimal PriceExclVat { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalInclVat { get; set; }
}