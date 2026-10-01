using Ardalis.Specification;
using Mkx.Templates.Domain.TestAggregate;

namespace Mkx.Templates.Infrastructure.Specifications.Tests;

public sealed class TestById : SingleResultSpecification<Test>
{
    public TestById(TestId id, bool tracking = false)
    {
        Query
            .Where(x => x.Id == id);
        if (!tracking) Query.AsNoTracking();
    }
}
