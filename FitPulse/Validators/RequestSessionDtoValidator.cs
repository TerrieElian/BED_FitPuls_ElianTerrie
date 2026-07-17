namespace FitPulse.Validators;

public class RequestSessionDtoValidator : AbstractValidator<RequestSessionDto>
{
    public RequestSessionDtoValidator()
    {
        RuleFor(r => r.DeviceType).IsInEnum();
    }
}