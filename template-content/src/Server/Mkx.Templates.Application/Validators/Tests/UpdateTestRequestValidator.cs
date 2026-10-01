using FluentValidation;
using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Shared.DTOs.Tests;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Application.Validators.Tests;

[ScopedService]
public sealed class UpdateTestRequestValidator : AbstractValidator<UpdateTestRequest>
{
    public UpdateTestRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Test.MaxNameLength);
        RuleFor(x => x.Description).MaximumLength(Test.MaxDescriptionLength);
        RuleFor(x => x.Version).NotEmpty();
    }
}
