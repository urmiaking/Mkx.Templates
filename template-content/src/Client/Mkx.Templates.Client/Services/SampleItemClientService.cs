using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Sdk.Shared.Exceptions;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.SampleItems;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Client.Services;

[ScopedService]
public class SampleItemClientService(HttpClient client, JsonSerializerOptions jsonOptions) : ISampleItemService
{
    public async Task<PagedList<GetSampleItemResponse>> GetAllAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.SampleItems.List(filter.Normalize()), cancellationToken);
        return await ReadAsync<PagedList<GetSampleItemResponse>>(response, cancellationToken);
    }
    public async Task<GetSampleItemResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.SampleItems.Get(id), cancellationToken);
        return await ReadAsync<GetSampleItemResponse>(response, cancellationToken);
    }
    public async Task<GetSampleItemResponse> CreateAsync(CreateSampleItemRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.SampleItems.Create(), request, jsonOptions, cancellationToken);
        return await ReadAsync<GetSampleItemResponse>(response, cancellationToken);
    }
    public async Task<GetSampleItemResponse> UpdateAsync(Guid id, UpdateSampleItemRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.SampleItems.Get(id), request, jsonOptions, cancellationToken);
        return await ReadAsync<GetSampleItemResponse>(response, cancellationToken);
    }
    public async Task DeleteAsync(Guid id, Guid version, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.SampleItems.Delete(id, version), cancellationToken);
        if (!response.IsSuccessStatusCode) throw await HttpRequestFailedException.GetExceptionAsync(response, cancellationToken);
    }
    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken token)
    {
        if (!response.IsSuccessStatusCode) throw await HttpRequestFailedException.GetExceptionAsync(response, token);
        return await response.Content.ReadFromJsonAsync<T>(jsonOptions, token) ?? throw new UnexpectedHttpResponseException();
    }
}
