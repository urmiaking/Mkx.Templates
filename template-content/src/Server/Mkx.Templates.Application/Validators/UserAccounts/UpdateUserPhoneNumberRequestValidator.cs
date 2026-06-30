using FluentValidation;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Shared.DTOs.UserAccounts;

namespace Mkx.Templates.Application.Validators.UserAccounts;

[ScopedService]
public sealed class UpdateUserPhoneNumberRequestValidator : AbstractValidator<UpdateUserPhoneNumberRequest>
{
    public UpdateUserPhoneNumberRequestValidator()
    {
        RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("شماره تماس الزامی است")
            .Matches(@"^09\d{9}$").WithMessage("شماره تلفن باید با 09 شروع شده و 11 رقم باشد");
    }
}
