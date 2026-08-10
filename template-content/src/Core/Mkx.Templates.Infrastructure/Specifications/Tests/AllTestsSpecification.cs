using Ardalis.Specification;
using Mkx.Templates.Domain.TestAggregate;

namespace Mkx.Templates.Infrastructure.Specifications.Tests;

public sealed class AllTestsSpecification : Specification<Test>
{
    public AllTestsSpecification()
    {
        Query.AsNoTracking();
    }
}
