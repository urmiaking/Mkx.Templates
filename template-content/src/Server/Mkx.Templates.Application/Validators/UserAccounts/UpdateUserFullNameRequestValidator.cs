using FluentValidation;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Shared.DTOs.UserAccounts;

namespace Mkx.Templates.Application.Validators.UserAccounts;

[ScopedService]
public sealed class UpdateUserFullNameRequestValidator : AbstractValidator<UpdateUserFullNameRequest>
{
    public UpdateUserFullNameRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام و نام خانوادگی نمی تواند خالی باشد")
            .MaximumLength(100).WithMessage("نام و نام خانوادگی نمی تواند بیشتر از 100 کاراکتر باشد");
    }
}
