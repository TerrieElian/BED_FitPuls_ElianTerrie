namespace FitPulse.DTO;

public class PaymentDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public decimal AmountExclVat { get; set; }
    public decimal VatAmount { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? BankTransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
}