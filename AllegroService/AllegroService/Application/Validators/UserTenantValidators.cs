using AllegroService.Application.DTOs.UserTenants;
using FluentValidation;

namespace AllegroService.Application.Validators;

public class CreateUserTenantRequestValidator : AbstractValidator<CreateUserTenantRequest>
{
    public CreateUserTenantRequestValidator()
    {
        RuleFor(x => x.FirebaseUid)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateUserTenantRequestValidator : AbstractValidator<UpdateUserTenantRequest>
{
    public UpdateUserTenantRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
