using AllegroService.Application.DTOs.Reservations;
using FluentValidation;

namespace AllegroService.Application.Validators;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.GuestId).NotEmpty();
        RuleFor(x => x.CheckInDate).NotEmpty();
        RuleFor(x => x.CheckOutDate).NotEmpty();
        RuleFor(x => x).Must(x => x.CheckOutDate > x.CheckInDate)
            .WithMessage("CheckOutDate must be greater than CheckInDate.");
        RuleFor(x => x.TotalEstimated).GreaterThanOrEqualTo(0);
    }
}

public class UpdateReservationRequestValidator : AbstractValidator<UpdateReservationRequest>
{
    public UpdateReservationRequestValidator()
    {
        RuleFor(x => x.GuestId).NotEmpty();
        RuleFor(x => x.CheckInDate).NotEmpty();
        RuleFor(x => x.CheckOutDate).NotEmpty();
        RuleFor(x => x).Must(x => x.CheckOutDate > x.CheckInDate)
            .WithMessage("CheckOutDate must be greater than CheckInDate.");
        RuleFor(x => x.TotalEstimated).GreaterThanOrEqualTo(0);
    }
}
