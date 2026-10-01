using Ardalis.Specification;
using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Sdk.Server.Shared.Data;
using SortDirection = Mkx.Templates.Sdk.Server.Shared.Enums.SortDirection;

namespace Mkx.Templates.Infrastructure.Specifications.Tests;

public sealed class TestsFilterSpecification : Specification<Test>
{
    public TestsFilterSpecification(RequestFilter filter, bool paginate = true)
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
