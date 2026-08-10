using Ardalis.Specification;
using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Shared.DTOs.Tests;

namespace Mkx.Templates.Application.Specifications.Tests;

public sealed class TestProjectionSpecification : Specification<Test, GetTestResponse>
{
    public TestProjectionSpecification()
    {
        Query
            .AsNoTracking()
            .Select(x => new GetTestResponse(x.Id.Value, x.Name, x.Description));
    }
}
