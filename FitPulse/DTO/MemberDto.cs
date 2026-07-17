namespace FitPulse.DTO;

public class MemberDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public string PreferredPaymentMethod { get; set; } = string.Empty;
    public string RfidTag { get; set; } = string.Empty;
}