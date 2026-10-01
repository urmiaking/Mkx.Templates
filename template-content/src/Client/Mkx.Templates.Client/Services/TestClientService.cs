using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Sdk.Shared.Exceptions;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Tests;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Client.Services;

[ScopedService]
public class TestClientService(HttpClient client, JsonSerializerOptions jsonOptions) : ITestService
{
    public async Task<PagedList<GetTestResponse>> GetAllAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Tests.List(filter.Normalize()), cancellationToken);
        return await ReadAsync<PagedList<GetTestResponse>>(response, cancellationToken);
    }
    public async Task<GetTestResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Tests.Get(id), cancellationToken);
        return await ReadAsync<GetTestResponse>(response, cancellationToken);
    }
    public async Task<GetTestResponse> CreateAsync(CreateTestRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Tests.Create(), request, jsonOptions, cancellationToken);
        return await ReadAsync<GetTestResponse>(response, cancellationToken);
    }
    public async Task<GetTestResponse> UpdateAsync(Guid id, UpdateTestRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Tests.Get(id), request, jsonOptions, cancellationToken);
        return await ReadAsync<GetTestResponse>(response, cancellationToken);
    }
    public async Task DeleteAsync(Guid id, Guid version, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.Tests.Delete(id, version), cancellationToken);
        if (!response.IsSuccessStatusCode) throw await HttpRequestFailedException.GetExceptionAsync(response, cancellationToken);
    }
    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken token)
    {
        if (!response.IsSuccessStatusCode) throw await HttpRequestFailedException.GetExceptionAsync(response, token);
        return await response.Content.ReadFromJsonAsync<T>(jsonOptions, token) ?? throw new UnexpectedHttpResponseException();
    }
}
