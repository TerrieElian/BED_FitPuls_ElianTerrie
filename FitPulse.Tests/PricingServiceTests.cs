namespace FitPulse.Tests;

public class PricingServiceTests
{
    private readonly IPricingService _pricingService = new PricingService();

    [Fact]
    public void BasisCardioSessie_GeenKortingGeenPiekuur_BerekentCorrecteTotaalprijs()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 60,
            CaloriesBurned = 300,
            DeviceType = DeviceType.Cardio,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 0,
            DiscountCode = null
        };

        var result = _pricingService.CalculatePrice(request);

        result.TotalInclVat.Should().Be(27.23m);
    }

    [Fact]
    public void StrengthSessieInPiekuur_PastPiekuurToeslagToe()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 30,
            CaloriesBurned = 200,
            DeviceType = DeviceType.Strength,
            SessionStartTime = new DateTime(2026, 1, 1, 18, 0, 0),
            LoyaltyPoints = 0,
            DiscountCode = null
        };

        var result = _pricingService.CalculatePrice(request);

        result.TotalInclVat.Should().Be(27.13m);
    }

    [Fact]
    public void PremiumSessieMetLoyaltyKorting_KapptOp20Procent()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 45,
            CaloriesBurned = 400,
            DeviceType = DeviceType.Premium,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 5000, // ruim genoeg om het 20%-plafond te raken
            DiscountCode = null
        };

        var result = _pricingService.CalculatePrice(request);

        result.LoyaltyDiscount.Should().Be(result.PriceAfterPeakHourSurcharge * 0.20m);
        result.TotalInclVat.Should().Be(44.19m);
    }

    [Fact]
    public void KortingscodePercentage_PastPercentageToe()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 60,
            CaloriesBurned = 0,
            DeviceType = DeviceType.Cardio,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 0,
            DiscountCode = new DiscountCode
            {
                Type = DiscountType.Percentage,
                Value = 15,
                ExpirationDate = new DateTime(2099, 1, 1),
                MinSessionPrice = 10.00m
            }
        };

        var result = _pricingService.CalculatePrice(request);

        result.TotalInclVat.Should().Be(16.97m);
    }

    [Fact]
    public void KortingscodeFlat_PastVastBedragToe()
    {
        // Dit resultaat (13.915 -> 13.92) test meteen ook het afrondings-randgeval:
        // een bedrag dat exact op een 2-decimalen-grens uitkomt.
        var request = new PricingRequest
        {
            DurationMinutes = 60,
            CaloriesBurned = 0,
            DeviceType = DeviceType.Cardio,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 0,
            DiscountCode = new DiscountCode
            {
                Type = DiscountType.FixedAmount,
                Value = 5.00m,
                ExpirationDate = new DateTime(2099, 1, 1),
                MinSessionPrice = 10.00m
            }
        };

        var result = _pricingService.CalculatePrice(request);

        result.TotalInclVat.Should().Be(13.92m);
    }

    [Fact]
    public void KortingscodeVerlopen_WordtGenegeerd()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 60,
            CaloriesBurned = 0,
            DeviceType = DeviceType.Cardio,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 0,
            DiscountCode = new DiscountCode
            {
                Type = DiscountType.FixedAmount,
                Value = 5.00m,
                ExpirationDate = new DateTime(2020, 1, 1), // in het verleden
                MinSessionPrice = 10.00m
            }
        };

        var result = _pricingService.CalculatePrice(request);

        result.DiscountCodeAmount.Should().Be(0m);
        result.TotalInclVat.Should().Be(19.97m);
    }

    [Fact]
    public void KortingscodeSessiewaardeTeLaag_WordtGenegeerd()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 60,
            CaloriesBurned = 0,
            DeviceType = DeviceType.Cardio,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 0,
            DiscountCode = new DiscountCode
            {
                Type = DiscountType.Percentage,
                Value = 15,
                ExpirationDate = new DateTime(2099, 1, 1),
                MinSessionPrice = 20.00m // hoger dan de sessieprijs van 16.50
            }
        };

        var result = _pricingService.CalculatePrice(request);

        result.DiscountCodeAmount.Should().Be(0m);
        result.TotalInclVat.Should().Be(19.97m);
    }

    [Fact]
    public void SessiePrijsOnderMinimum_MinimumtariefWordtAfgedwongen()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 1,
            CaloriesBurned = 0,
            DeviceType = DeviceType.Cardio,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 0,
            DiscountCode = null
        };

        var result = _pricingService.CalculatePrice(request);

        result.TotalInclVat.Should().Be(4.00m);
    }

    [Fact]
    public void LoyaltyEnKortingscodeSamen_KortingscodeWerktOpBedragNaLoyalty()
    {
        var request = new PricingRequest
        {
            DurationMinutes = 60,
            CaloriesBurned = 300,
            DeviceType = DeviceType.Cardio,
            SessionStartTime = new DateTime(2026, 1, 1, 10, 0, 0),
            LoyaltyPoints = 300,
            DiscountCode = new DiscountCode
            {
                Type = DiscountType.Percentage,
                Value = 10,
                ExpirationDate = new DateTime(2099, 1, 1),
                MinSessionPrice = 10.00m
            }
        };

        var result = _pricingService.CalculatePrice(request);

        result.TotalInclVat.Should().Be(21.24m);
    }
}