namespace FitPulse.Services;

public interface IPricingService
{
    PricingResult CalculatePrice(PricingRequest request);
}

public class PricingService : IPricingService
{
    private const decimal SessionRate = 1.50m;
    private const decimal RatePerMinute = 0.25m;
    private const decimal RatePerKcal = 0.02m;
    private const decimal PeakHourSurchargePercentage = 0.15m;
    private const int PeakHourStart = 17;
    private const int PeakHourEnd = 21;
    private const decimal LoyaltyRatePer100Points = 1.00m;
    private const decimal LoyaltyMaxPercentage = 0.20m;
    private const decimal VatRate = 0.21m;
    private const decimal MinimumPrice = 4.00m;

    public PricingResult CalculatePrice(PricingRequest request)
    {
        // Stap 1: basisprijs
        var basePrice = SessionRate
            + (request.DurationMinutes * RatePerMinute)
            + (request.CaloriesBurned * RatePerKcal);

        // Stap 2: multiplier toesteltype
        var multiplier = GetDeviceTypeMultiplier(request.DeviceType);
        var priceAfterMultiplier = basePrice * multiplier;

        // Stap 3: piekuur-opslag (op de prijs ná stap 2)
        var isPeakHour = request.SessionStartTime.Hour >= PeakHourStart && request.SessionStartTime.Hour < PeakHourEnd;
        var priceAfterPeakHour = isPeakHour
            ? priceAfterMultiplier * (1 + PeakHourSurchargePercentage)
            : priceAfterMultiplier;

        // Stap 4: loyalty-korting, geplafonneerd op 20% van de huidige prijs
        var rawLoyaltyDiscount = (request.LoyaltyPoints / 100m) * LoyaltyRatePer100Points;
        var maxLoyaltyDiscount = priceAfterPeakHour * LoyaltyMaxPercentage;
        var loyaltyDiscount = Math.Min(rawLoyaltyDiscount, maxLoyaltyDiscount);
        var priceAfterLoyalty = priceAfterPeakHour - loyaltyDiscount;

        // Stap 5: kortingscode valideren, dan toepassen op het bedrag ná loyalty-korting
        var discountCodeAmount = CalculateDiscountCodeAmount(request.DiscountCode, priceAfterLoyalty);
        var priceExclVat = priceAfterLoyalty - discountCodeAmount;

        // Stap 6: BTW toevoegen
        var vatAmount = priceExclVat * VatRate;
        var totalInclVat = priceExclVat + vatAmount;

        // Stap 7: minimumprijs afdwingen
        totalInclVat = Math.Max(totalInclVat, MinimumPrice);

        // Stap 8: nooit negatief (expliciete check, ook al dekt stap 7 dit eigenlijk af)
        totalInclVat = Math.Max(totalInclVat, 0m);

        // Stap 9: afronden op 2 decimalen — pas als allerlaatste stap
        totalInclVat = Math.Round(totalInclVat, 2, MidpointRounding.AwayFromZero);

        return new PricingResult
        {
            BasePrice = basePrice,
            PriceAfterMultiplier = priceAfterMultiplier,
            PriceAfterPeakHourSurcharge = priceAfterPeakHour,
            LoyaltyDiscount = loyaltyDiscount,
            DiscountCodeAmount = discountCodeAmount,
            PriceExclVat = priceExclVat,
            VatAmount = vatAmount,
            TotalInclVat = totalInclVat
        };
    }

    private static decimal GetDeviceTypeMultiplier(DeviceType deviceType) => deviceType switch
    {
        DeviceType.Cardio => 1.0m,
        DeviceType.Strength => 1.5m,
        DeviceType.Premium => 2.2m,
        _ => throw new ArgumentOutOfRangeException(nameof(deviceType))
    };

    private static decimal CalculateDiscountCodeAmount(DiscountCode? discountCode, decimal priceBeforeDiscountCode)
    {
        if (discountCode is null)
        {
            return 0m;
        }

        var isExpired = DateTime.UtcNow > discountCode.ExpirationDate;
        var meetsMinimum = priceBeforeDiscountCode > discountCode.MinSessionPrice;

        if (isExpired || !meetsMinimum)
        {
            return 0m;
        }

        return discountCode.Type switch
        {
            DiscountType.Percentage => priceBeforeDiscountCode * (discountCode.Value / 100m),
            DiscountType.FixedAmount => discountCode.Value,
            _ => 0m
        };
    }
}