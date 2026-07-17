namespace FitPulse.Tests;

public class PaymentServiceTests
{
    [Fact]
    public async Task GenerateForSessionAsync_SessieNietGevonden_GooitKeyNotFoundException()
    {
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TrainingSession?)null);

        var service = new PaymentService(
            sessionRepositoryMock.Object,
            Mock.Of<IMemberRepository>(),
            Mock.Of<IDiscountCodeRepository>(),
            Mock.Of<IPaymentRepository>(),
            new PricingService());

        Func<Task> act = () => service.GenerateForSessionAsync(999, null);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GenerateForSessionAsync_SessieNietVoltooid_GooitInvalidOperationException()
    {
        var session = new TrainingSession
        {
            Id = 1,
            Status = TrainingSessionStatus.InProgress,
            Device = new Device { DeviceType = DeviceType.Cardio },
            Member = new Member { LoyaltyPoints = 0 }
        };
        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);

        var service = new PaymentService(
            sessionRepositoryMock.Object,
            Mock.Of<IMemberRepository>(),
            Mock.Of<IDiscountCodeRepository>(),
            Mock.Of<IPaymentRepository>(),
            new PricingService());

        Func<Task> act = () => service.GenerateForSessionAsync(1, null);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateForSessionAsync_VoltooideSessieZonderKorting_MaaktPaymentEnTrektLoyaltyPuntenAf()
    {
        var member = new Member { Id = 1, LoyaltyPoints = 300 };
        var session = new TrainingSession
        {
            Id = 1,
            Status = TrainingSessionStatus.Completed,
            RequestedAt = new DateTime(2026, 1, 1, 10, 0, 0),
            DurationMinutes = 60,
            CaloriesBurned = 300,
            Device = new Device { DeviceType = DeviceType.Cardio },
            Member = member
        };

        var sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        sessionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        var memberRepositoryMock = new Mock<IMemberRepository>();
        var paymentRepositoryMock = new Mock<IPaymentRepository>();

        var service = new PaymentService(
            sessionRepositoryMock.Object,
            memberRepositoryMock.Object,
            Mock.Of<IDiscountCodeRepository>(),
            paymentRepositoryMock.Object,
            new PricingService());

        var payment = await service.GenerateForSessionAsync(1, null);

        payment.SessionId.Should().Be(1);
        payment.Amount.Should().Be(23.60m); // €22,50 basis - €3,00 loyalty-korting, incl. 21% BTW // zelfde scenario als de "basis cardio"-test uit PricingServiceTests
        member.LoyaltyPoints.Should().Be(0); // 300 punten = €3 korting, binnen het 20%-plafond, dus volledig gebruikt
        memberRepositoryMock.Verify(r => r.UpdateAsync(member), Times.Once);
        paymentRepositoryMock.Verify(r => r.AddAsync(payment), Times.Once);
    }
}