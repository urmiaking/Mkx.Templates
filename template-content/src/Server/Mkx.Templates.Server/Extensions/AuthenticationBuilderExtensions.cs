using Microsoft.AspNetCore.Authentication;
using Mkx.Templates.Server.Common;

namespace Mkx.Templates.Server.Extensions;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddOidcProviders(
        this AuthenticationBuilder authBuilder,
        IConfiguration configuration)
    {
        var providers = configuration
            .GetSection("Authentication:ExternalProviders")
            .Get<List<OidcConfig>>();

        if (providers == null || providers.Count == 0)
        {
            return authBuilder;
        }

        foreach (var provider in providers)
        {
            authBuilder.AddOpenIdConnect(provider.Scheme, provider.DisplayName, o =>
            {
                o.Authority = provider.Authority;
                o.ClientId = provider.ClientId;
                o.ClientSecret = provider.ClientSecret;

                o.ResponseType = "code";
                o.SaveTokens = true;
                o.MapInboundClaims = false;
                o.CallbackPath = $"/{provider.RedirectUrl}";

                o.Scope.Clear();
                foreach (var scope in provider.Scopes ?? ["openid", "profile"])
                    o.Scope.Add(scope);
#if DEBUG
                o.RequireHttpsMetadata = false;
#endif
            });
        }

        return authBuilder;
    }
}