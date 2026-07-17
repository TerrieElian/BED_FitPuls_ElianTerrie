namespace FitPulse.Validators;

public class MaintenanceLogValidator : AbstractValidator<MaintenanceLog>
{
    public MaintenanceLogValidator()
    {
        RuleFor(m => m.DeviceId).GreaterThan(0);
        RuleFor(m => m.Date).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(m => m.Description).NotEmpty();
        RuleFor(m => m.TechnicianName).NotEmpty();
        RuleFor(m => m.Cost).GreaterThanOrEqualTo(0);
        RuleFor(m => m.NextServiceDue)
            .GreaterThanOrEqualTo(m => m.Date)
            .When(m => m.NextServiceDue.HasValue);
    }
}