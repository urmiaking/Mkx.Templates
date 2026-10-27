using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Client.Services;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Extensions;
using Mkx.Templates.Sdk.Shared.Utilities;
using Mkx.Templates.Client.Components.UI;
using Mkx.Templates.Shared.Authorization;

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
                .AddHttpClientService(environment, TimeSpan.FromSeconds(30))
                .AddJsonOptions();

            return services;
        }

        public IServiceCollection AddClientServerServices()
        {
            services.AddBlazoredLocalStorage();

            services.InitializeDefaultCulture();

            services.AddScoped<DialogService>();
            services.AddScoped<ToastService>();

            services.AddScoped<ThemeService>();
            services.AddScoped<BrowserPushService>();
            services.AddScoped<WebAuthnService>();

            return services;
        }

        public IServiceCollection AddHttpClientService(IWebAssemblyHostEnvironment environment, TimeSpan timeOut)
        {
            var baseAddress = environment.BaseAddress;
            services.AddScoped(sp =>
            {
                var client = new HttpClient(new AntiforgeryHandler(new Uri(baseAddress))) { BaseAddress = new Uri(baseAddress) };
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
            services.AddSingleton<IApplicationPolicyProvider, AppPolicyProvider>();
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddScoped<PersistentAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<PersistentAuthenticationStateProvider>());

            return services;
        }
    }
}