namespace FitPulse.Models;

public class Member : User
{
    public Member()
    {
        Role = UserRole.Member;
    }

    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public string PreferredPaymentMethod { get; set; } = string.Empty;
    public string RfidTag { get; set; } = string.Empty;
}
