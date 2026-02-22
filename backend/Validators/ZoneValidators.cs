using FluentValidation;
using EvacuationAPI.DTOs;

namespace EvacuationAPI.Validators
{
    public class CreateZoneRequestValidator : AbstractValidator<CreateZoneRequest>
    {
        public CreateZoneRequestValidator()
        {
            RuleFor(x => x.ZoneId)
                .NotEmpty().WithMessage("ZoneId is required.");

            RuleFor(x => x.LocationCoordinates)
                .NotNull().WithMessage("LocationCoordinates are required.")
                .SetValidator(new LocationCoordinatesValidator());

            RuleFor(x => x.NumberOfPeople)
                .GreaterThan(0).WithMessage("NumberOfPeople must be greater than 0.");

            RuleFor(x => x.UrgencyLevel)
                .InclusiveBetween(1, 5).WithMessage("UrgencyLevel must be between 1 and 5.");
        }
    }

    public class UpdateZoneRequestValidator : AbstractValidator<UpdateZoneRequest>
    {
        public UpdateZoneRequestValidator()
        {
            RuleFor(x => x.LocationCoordinates)
                .SetValidator(new LocationCoordinatesValidator()!)
                .When(x => x.LocationCoordinates != null);

            RuleFor(x => x.NumberOfPeople)
                .GreaterThan(0).When(x => x.NumberOfPeople.HasValue).WithMessage("NumberOfPeople must be greater than 0.");

            RuleFor(x => x.UrgencyLevel)
                .InclusiveBetween(1, 5).When(x => x.UrgencyLevel.HasValue).WithMessage("UrgencyLevel must be between 1 and 5.");
        }
    }
}
