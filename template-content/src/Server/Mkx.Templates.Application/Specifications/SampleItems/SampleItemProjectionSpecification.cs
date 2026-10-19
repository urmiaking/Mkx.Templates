using Ardalis.Specification;
using Mkx.Templates.Domain.SampleItemAggregate;
using Mkx.Templates.Shared.DTOs.SampleItems;

namespace Mkx.Templates.Application.Specifications.SampleItems;

public sealed class SampleItemProjectionSpecification : Specification<SampleItem, GetSampleItemResponse>
{
    public SampleItemProjectionSpecification()
    {
        Query
            .AsNoTracking()
            .Select(x => new GetSampleItemResponse(x.Id.Value, x.Name, x.Description, x.Version));
    }
}
