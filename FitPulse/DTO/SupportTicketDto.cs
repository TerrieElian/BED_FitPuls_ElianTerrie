namespace FitPulse.DTO;

public class SupportTicketDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}