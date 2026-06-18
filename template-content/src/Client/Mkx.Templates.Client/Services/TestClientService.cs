using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Sdk.Shared.Exceptions;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Tests;
using Mkx.Templates.Shared.Routes;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mkx.Templates.Client.Services;

[ScopedService]
public class TestClientService(HttpClient client, JsonSerializerOptions jsonOptions) : ITestService
{
    public async Task<List<GetTestResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiRoutes.Tests.Base, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetTestResponse>>(jsonOptions, cancellationToken);

        return result ?? [];
    }

    public async Task<GetTestResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Tests.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetTestResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}
