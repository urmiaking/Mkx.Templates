using FluentValidation;
using Mkx.Templates.Domain.SampleItemAggregate;
using Mkx.Templates.Shared.DTOs.SampleItems;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Application.Validators.SampleItems;

[ScopedService]
public sealed class UpdateSampleItemRequestValidator : AbstractValidator<UpdateSampleItemRequest>
{
    public UpdateSampleItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(SampleItem.MaxNameLength);
        RuleFor(x => x.Description).MaximumLength(SampleItem.MaxDescriptionLength);
        RuleFor(x => x.Version).NotEmpty();
    }
}
