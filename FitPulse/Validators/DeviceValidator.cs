

namespace FitPulse.Validators;

public class DeviceValidator : AbstractValidator<Device>
{
    public DeviceValidator()
    {
        RuleFor(d => d.SerialNumber).NotEmpty().MaximumLength(50);
        RuleFor(d => d.LocationCode).NotEmpty();
        RuleFor(d => d.Model).NotEmpty();
        RuleFor(d => d.InstallationYear).InclusiveBetween(2000, DateTime.UtcNow.Year);
        RuleFor(d => d.DeviceType).IsInEnum();
    }
}