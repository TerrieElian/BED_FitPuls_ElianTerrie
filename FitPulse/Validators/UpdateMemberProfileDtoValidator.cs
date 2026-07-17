namespace FitPulse.Validators;

public class UpdateMemberProfileDtoValidator : AbstractValidator<UpdateMemberProfileDto>
{
    public UpdateMemberProfileDtoValidator()
    {
        RuleFor(m => m.Email).NotEmpty().EmailAddress();
    }
}
