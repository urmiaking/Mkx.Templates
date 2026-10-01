using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Shared.DTOs.Tests;

namespace Mkx.Templates.Shared.Abstractions;

public interface ITestService
{
    Task<PagedList<GetTestResponse>> GetAllAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetTestResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetTestResponse> CreateAsync(CreateTestRequest request, CancellationToken cancellationToken = default);
    Task<GetTestResponse> UpdateAsync(Guid id, UpdateTestRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid version, CancellationToken cancellationToken = default);
}
