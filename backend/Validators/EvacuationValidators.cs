using FluentValidation;
using EvacuationAPI.DTOs;

namespace EvacuationAPI.Validators
{
    public class UpdateEvacuationRequestValidator : AbstractValidator<UpdateEvacuationRequest>
    {
        public UpdateEvacuationRequestValidator()
        {
            RuleFor(x => x.ZoneId).NotEmpty().WithMessage("ZoneId is required.");
            RuleFor(x => x.VehicleId).NotEmpty().WithMessage("VehicleId is required.");
            RuleFor(x => x.PeopleEvacuated).GreaterThan(0).WithMessage("PeopleEvacuated must be greater than 0.");
        }
    }

    public class ClearDataRequestValidator : AbstractValidator<ClearDataRequest>
    {
        public ClearDataRequestValidator()
        {
            RuleFor(x => x.Confirmation)
                .NotEmpty().WithMessage("Confirmation is required.")
                .Equal("CLEAR_ALL_DATA").WithMessage("Confirmation must be 'CLEAR_ALL_DATA' to proceed.");
        }
    }
}
