using MapsterMapper;
using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Infrastructure.Repositories.Abstractions;
using Mkx.Templates.Infrastructure.Specifications.Tests;
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Tests;

namespace Mkx.Templates.Application.Services;

[ScopedService]
internal sealed class TestService(
    ITestRepository testRepository,
    IMapper mapper) : ITestService
{
    public async Task<List<GetTestResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tests = await testRepository.ListAsync(new AllTestsSpecification(), cancellationToken);

        return mapper.Map<List<GetTestResponse>>(tests);
    }

    public async Task<GetTestResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var test = await testRepository.SingleOrDefaultAsync(new TestById(new TestId(id)), cancellationToken)
                   ?? throw new NotFoundException();

        return mapper.Map<GetTestResponse>(test);
    }
}
