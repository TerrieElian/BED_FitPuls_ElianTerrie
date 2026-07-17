namespace FitPulse.Validators;

public class CompleteSessionDtoValidator : AbstractValidator<CompleteSessionDto>
{
    public CompleteSessionDtoValidator()
    {
        RuleFor(c => c.CaloriesBurned).GreaterThanOrEqualTo(0);
    }
}