using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Mkx.Templates.Application.Validators.SampleItems;
using Mkx.Templates.Domain.SampleItemAggregate;
using Mkx.Templates.Infrastructure.Repositories.Abstractions;
using Mkx.Templates.Infrastructure.Specifications.SampleItems;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.SampleItems;

namespace Mkx.Templates.Application.Services;

[ScopedService]
internal sealed class SampleItemService(ISampleItemRepository repository, IMapper mapper,
    CreateSampleItemRequestValidator createValidator, UpdateSampleItemRequestValidator updateValidator) : ISampleItemService
{
    public async Task<PagedList<GetSampleItemResponse>> GetAllAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        filter = filter.Normalize();
        var total = await repository.CountAsync(new SampleItemsFilterSpecification(filter, paginate: false), cancellationToken);
        var items = await repository.ListAsync(new SampleItemsFilterSpecification(filter), cancellationToken);
        return new() { Data = mapper.Map<List<GetSampleItemResponse>>(items), Total = total, Skip = filter.Skip!.Value, Take = filter.Take!.Value };
    }
    public async Task<GetSampleItemResponse> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        mapper.Map<GetSampleItemResponse>(await RequiredAsync(id, false, cancellationToken));
    public async Task<GetSampleItemResponse> CreateAsync(CreateSampleItemRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var item = SampleItem.Create(request.Name, request.Description);
        await repository.CreateAsync(item, cancellationToken);
        return mapper.Map<GetSampleItemResponse>(item);
    }
    public async Task<GetSampleItemResponse> UpdateAsync(Guid id, UpdateSampleItemRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var item = await RequiredAsync(id, true, cancellationToken);
        EnsureVersion(item, request.Version);
        item.Update(request.Name, request.Description);
        await repository.SaveAsync(cancellationToken);
        return mapper.Map<GetSampleItemResponse>(item);
    }
    public async Task DeleteAsync(Guid id, Guid version, CancellationToken cancellationToken = default)
    {
        var item = await RequiredAsync(id, true, cancellationToken);
        EnsureVersion(item, version);
        await repository.DeleteAsync(item, cancellationToken);
    }
    private async Task<SampleItem> RequiredAsync(Guid id, bool tracking, CancellationToken token) =>
        await repository.SingleOrDefaultAsync(new SampleItemById(new SampleItemId(id), tracking), token) ?? throw new NotFoundException("Sample item not found.");
    private static void EnsureVersion(SampleItem item, Guid version)
    {
        if (version == Guid.Empty) throw new ArgumentException("A version is required.", nameof(version));
        if (version != item.Version) throw new DbUpdateConcurrencyException();
    }
}
