namespace FitPulse.Validators;

public class CreateTicketDtoValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketDtoValidator()
    {
        RuleFor(t => t.Subject).NotEmpty().MaximumLength(150);
        RuleFor(t => t.Description).NotEmpty();
        RuleFor(t => t.Priority).IsInEnum();
    }
}