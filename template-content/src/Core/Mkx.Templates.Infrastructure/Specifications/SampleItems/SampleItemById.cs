using Ardalis.Specification;
using Mkx.Templates.Domain.SampleItemAggregate;

namespace Mkx.Templates.Infrastructure.Specifications.SampleItems;

public sealed class SampleItemById : SingleResultSpecification<SampleItem>
{
    public SampleItemById(SampleItemId id, bool tracking = false)
    {
        Query
            .Where(x => x.Id == id);
        if (!tracking) Query.AsNoTracking();
    }
}
