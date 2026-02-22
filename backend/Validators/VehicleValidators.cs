using FluentValidation;
using EvacuationAPI.DTOs;

namespace EvacuationAPI.Validators
{
    public class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
    {
        public CreateVehicleRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.VehicleId)
                .NotEmpty().WithMessage("VehicleId is required.");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.")
                .Must(type => !string.IsNullOrEmpty(type) && new[] { "bus", "van", "boat" }.Contains(type.ToLower()))
                .WithMessage("Type must be 'bus', 'van', or 'boat'.");

            RuleFor(x => x.LocationCoordinates)
                .NotNull().WithMessage("LocationCoordinates are required.")
                .SetValidator(new LocationCoordinatesValidator());

            RuleFor(x => x.Speed)
                .GreaterThan(0).WithMessage("Speed must be greater than 0.");
        }
    }

    public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
    {
        public UpdateVehicleRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Capacity)
                .GreaterThan(0).When(x => x.Capacity.HasValue).WithMessage("Capacity must be greater than 0.");

            RuleFor(x => x.Type)
                .Must(type => type == null || new[] { "bus", "van", "boat" }.Contains(type.ToLower()))
                .WithMessage("Type must be 'bus', 'van', or 'boat'.");

            RuleFor(x => x.LocationCoordinates)
                .SetValidator(new LocationCoordinatesValidator()!)
                .When(x => x.LocationCoordinates != null);

            RuleFor(x => x.Speed)
                .GreaterThan(0).When(x => x.Speed.HasValue).WithMessage("Speed must be greater than 0.");
        }
    }
}
