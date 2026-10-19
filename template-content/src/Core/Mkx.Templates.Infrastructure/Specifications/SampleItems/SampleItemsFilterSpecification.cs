using Ardalis.Specification;
using Mkx.Templates.Domain.SampleItemAggregate;
using Mkx.Templates.Sdk.Server.Shared.Data;
using SortDirection = Mkx.Templates.Sdk.Server.Shared.Enums.SortDirection;

namespace Mkx.Templates.Infrastructure.Specifications.SampleItems;

public sealed class SampleItemsFilterSpecification : Specification<SampleItem>
{
    public SampleItemsFilterSpecification(RequestFilter filter, bool paginate = true)
    {
        filter = filter.Normalize();
        Query.AsNoTracking();
        if (filter.Search is { } search)
            Query.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
        if (filter.SortDirection == SortDirection.Descending)
            Query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id);
        else Query.OrderBy(x => x.Name).ThenBy(x => x.Id);
        if (paginate) Query.Skip(filter.Skip!.Value).Take(filter.Take!.Value);
    }
}
