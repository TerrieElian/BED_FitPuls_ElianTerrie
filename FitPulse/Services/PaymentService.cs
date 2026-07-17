namespace FitPulse.Services;

public interface IPaymentService
{
    Task<Payment> GenerateForSessionAsync(int sessionId, string? discountCode);
}

public class PaymentService : IPaymentService
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPricingService _pricingService;

    public PaymentService(
        ITrainingSessionRepository sessionRepository,
        IMemberRepository memberRepository,
        IDiscountCodeRepository discountCodeRepository,
        IPaymentRepository paymentRepository,
        IPricingService pricingService)
    {
        _sessionRepository = sessionRepository;
        _memberRepository = memberRepository;
        _discountCodeRepository = discountCodeRepository;
        _paymentRepository = paymentRepository;
        _pricingService = pricingService;
    }

    public async Task<Payment> GenerateForSessionAsync(int sessionId, string? discountCode)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session is null)
        {
            throw new KeyNotFoundException($"Sessie {sessionId} niet gevonden.");
        }

        if (session.Status != TrainingSessionStatus.Completed)
        {
            throw new InvalidOperationException("Kan enkel een betaling genereren voor een voltooide sessie.");
        }

        DiscountCode? discountCodeEntity = null;
        if (!string.IsNullOrWhiteSpace(discountCode))
        {
            discountCodeEntity = await _discountCodeRepository.GetByCodeAsync(discountCode);
        }

        var pricingRequest = new PricingRequest
        {
            DurationMinutes = session.DurationMinutes ?? 0,
            CaloriesBurned = session.CaloriesBurned ?? 0,
            DeviceType = session.Device.DeviceType,
            SessionStartTime = session.StartedAt ?? session.RequestedAt,
            LoyaltyPoints = session.Member.LoyaltyPoints,
            DiscountCode = discountCodeEntity
        };

        var pricingResult = _pricingService.CalculatePrice(pricingRequest);

        var pointsUsed = (int)(pricingResult.LoyaltyDiscount * 100);
        session.Member.LoyaltyPoints -= pointsUsed;
        await _memberRepository.UpdateAsync(session.Member);

        var payment = new Payment
        {
            SessionId = session.Id,
            AmountExclVat = pricingResult.PriceExclVat,
            VatAmount = pricingResult.VatAmount,
            Amount = pricingResult.TotalInclVat,
            Currency = "EUR",
            Status = PaymentStatus.Completed,
            BankTransactionRef = Guid.NewGuid().ToString(),
            PaidAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);
        return payment;
    }
}