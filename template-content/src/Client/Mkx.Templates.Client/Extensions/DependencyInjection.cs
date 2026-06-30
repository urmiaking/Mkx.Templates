using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Client.Services;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Extensions;
using Mkx.Templates.Sdk.Shared.Utilities;
using MudBlazor.Services;

namespace Mkx.Templates.Client.Extensions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClient(IWebAssemblyHostEnvironment environment)
        {
            services.AddClientServerServices()
                .DiscoverServices()
                .AddAuthServices()
                .AddHttpClientService(environment, new TimeSpan(0, 10, 0))
                .AddJsonOptions();

            return services;
        }

        public IServiceCollection AddClientServerServices()
        {
            services.AddBlazoredLocalStorage();

            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomLeft;
                config.SnackbarConfiguration.HideTransitionDuration = 1500;
                config.PopoverOptions.Duration = TimeSpan.FromMilliseconds(500);
            });

            services.AddScoped<ThemeService>();
            services.AddScoped<WebAuthnService>();

            return services;
        }

        public IServiceCollection AddHttpClientService(IWebAssemblyHostEnvironment environment, TimeSpan timeOut)
        {
            var baseAddress = environment.BaseAddress;
            services.AddScoped(sp =>
            {
                var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
                client.Timeout = timeOut;
                return client;
            });

            return services;
        }

        public IServiceCollection AddJsonOptions()
        {
            var jsonOptions = JsonOptionHelpers.GetJsonOptions();
            services.AddSingleton(jsonOptions);

            return services;
        }

        public IServiceCollection AddAuthServices()
        {
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

            return services;
        }
    }
}
