using AllegroService.Application.DTOs.Folios;
using AllegroService.Application.DTOs.Stays;
using FluentValidation;

namespace AllegroService.Application.Validators;

public class CheckInRequestValidator : AbstractValidator<CheckInRequest>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.RoomUnitPrice).GreaterThanOrEqualTo(0).When(x => x.RoomUnitPrice.HasValue);
        RuleFor(x => x.RoomNights).GreaterThan(0).When(x => x.RoomNights.HasValue);
    }
}

public class CheckOutRequestValidator : AbstractValidator<CheckOutRequest>
{
}

public class AddChargeItemRequestValidator : AbstractValidator<AddChargeItemRequest>
{
    public AddChargeItemRequestValidator()
    {
        RuleFor(x => x.Qty).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x).Must(x => x.ProductId.HasValue || x.UnitPrice.HasValue)
            .WithMessage("UnitPrice is required when ProductId is not provided.");
    }
}

public class AddChargeRequestValidator : AbstractValidator<AddChargeRequest>
{
    public AddChargeRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Items).ForEach(item => item.SetValidator(new AddChargeItemRequestValidator()));
        RuleFor(x => x.Qty).GreaterThan(0).When(x => x.Items.Count == 0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.Items.Count == 0);
        RuleFor(x => x).Must(x => x.Items.Count > 0 || (x.Qty.HasValue && x.UnitPrice.HasValue))
            .WithMessage("Provide charge items or a direct Qty and UnitPrice.");
    }
}

public class AddPaymentRequestValidator : AbstractValidator<AddPaymentRequest>
{
    public AddPaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reference).MaximumLength(100);
    }
}
