namespace FitPulse.Validators;

public class UpdateTicketStatusDtoValidator : AbstractValidator<UpdateTicketStatusDto>
{
    public UpdateTicketStatusDtoValidator()
    {
        RuleFor(t => t.Status).IsInEnum();
    }
}