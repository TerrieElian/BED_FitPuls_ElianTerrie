namespace FitPulse.Models;

public class Payment
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public TrainingSession Session { get; set; } = null!;
    public decimal AmountExclVat { get; set; }
    public decimal VatAmount { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? BankTransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
}
