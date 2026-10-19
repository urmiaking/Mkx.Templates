using Ardalis.Specification;
using Mkx.Templates.Domain.SampleItemAggregate;

namespace Mkx.Templates.Infrastructure.Specifications.SampleItems;

public sealed class AllSampleItemsSpecification : Specification<SampleItem>
{
    public AllSampleItemsSpecification()
    {
        Query.AsNoTracking();
    }
}
