using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Shared.DTOs.SampleItems;

namespace Mkx.Templates.Shared.Abstractions;

public interface ISampleItemService
{
    Task<PagedList<GetSampleItemResponse>> GetAllAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetSampleItemResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetSampleItemResponse> CreateAsync(CreateSampleItemRequest request, CancellationToken cancellationToken = default);
    Task<GetSampleItemResponse> UpdateAsync(Guid id, UpdateSampleItemRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid version, CancellationToken cancellationToken = default);
}
