using System.Net.Http.Json;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Client.Common;

// Tokens are acquired for each mutation: they are identity-bound and must not survive login/logout.
public sealed class AntiforgeryHandler(Uri baseAddress) : DelegatingHandler(new HttpClientHandler())
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var target = request.RequestUri is { IsAbsoluteUri: true } absolute
            ? absolute : new Uri(baseAddress, request.RequestUri!);
        if (target.GetLeftPart(UriPartial.Authority) != baseAddress.GetLeftPart(UriPartial.Authority))
            throw new InvalidOperationException("The application HTTP client only sends requests to its own origin.");

        if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head && request.Method != HttpMethod.Options)
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(baseAddress, ApiUrls.Accounts.Antiforgery()));
            using var tokenResponse = await base.SendAsync(tokenRequest, cancellationToken);
            tokenResponse.EnsureSuccessStatusCode();
            var token = await tokenResponse.Content.ReadFromJsonAsync<AntiforgeryResponse>(cancellationToken);
            if (string.IsNullOrWhiteSpace(token?.RequestToken))
                throw new InvalidOperationException("The server did not supply an antiforgery token.");
            request.Headers.Remove("X-CSRF-TOKEN");
            request.Headers.Add("X-CSRF-TOKEN", token.RequestToken);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
