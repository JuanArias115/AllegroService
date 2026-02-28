using AllegroService.Application.DTOs.Common;
using FluentValidation;

namespace AllegroService.Application.Validators;

public class ListQueryRequestValidator : AbstractValidator<ListQueryRequest>
{
    public ListQueryRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
