using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Mkx.Templates.Application.Validators.Tests;
using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Infrastructure.Repositories.Abstractions;
using Mkx.Templates.Infrastructure.Specifications.Tests;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Tests;

namespace Mkx.Templates.Application.Services;

[ScopedService]
internal sealed class TestService(ITestRepository repository, IMapper mapper,
    CreateTestRequestValidator createValidator, UpdateTestRequestValidator updateValidator) : ITestService
{
    public async Task<PagedList<GetTestResponse>> GetAllAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        filter = filter.Normalize();
        var total = await repository.CountAsync(new TestsFilterSpecification(filter, paginate: false), cancellationToken);
        var tests = await repository.ListAsync(new TestsFilterSpecification(filter), cancellationToken);
        return new() { Data = mapper.Map<List<GetTestResponse>>(tests), Total = total, Skip = filter.Skip!.Value, Take = filter.Take!.Value };
    }
    public async Task<GetTestResponse> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        mapper.Map<GetTestResponse>(await RequiredAsync(id, false, cancellationToken));
    public async Task<GetTestResponse> CreateAsync(CreateTestRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var test = Test.Create(request.Name, request.Description);
        await repository.CreateAsync(test, cancellationToken);
        return mapper.Map<GetTestResponse>(test);
    }
    public async Task<GetTestResponse> UpdateAsync(Guid id, UpdateTestRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var test = await RequiredAsync(id, true, cancellationToken);
        EnsureVersion(test, request.Version);
        test.Update(request.Name, request.Description);
        await repository.SaveAsync(cancellationToken);
        return mapper.Map<GetTestResponse>(test);
    }
    public async Task DeleteAsync(Guid id, Guid version, CancellationToken cancellationToken = default)
    {
        var test = await RequiredAsync(id, true, cancellationToken);
        EnsureVersion(test, version);
        await repository.DeleteAsync(test, cancellationToken);
    }
    private async Task<Test> RequiredAsync(Guid id, bool tracking, CancellationToken token) =>
        await repository.SingleOrDefaultAsync(new TestById(new TestId(id), tracking), token) ?? throw new NotFoundException("Test not found.");
    private static void EnsureVersion(Test test, Guid version)
    {
        if (version == Guid.Empty) throw new ArgumentException("A version is required.", nameof(version));
        if (version != test.Version) throw new DbUpdateConcurrencyException();
    }
}
