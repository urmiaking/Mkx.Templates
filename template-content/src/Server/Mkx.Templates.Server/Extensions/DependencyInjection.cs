using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Mkx.Templates.Sdk.Server.Api.Extensions;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Extensions;
using Mkx.Templates.Server.Common;
using Mkx.Templates.Server.Services;
using Mkx.Templates.Shared.Authorization;

namespace Mkx.Templates.Server.Extensions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddServer(IConfiguration configuration)
        {
            services.InitializeDefaultCulture()
                .AddControllers(configuration)
                .AddComponents()
                .AddSwagger()
                .AddCache()
                .AddAuth(configuration)
                .AddAuthOptions(configuration)
                .AddOidcProviderOptions(configuration)
                .AddSerilogUiService(configuration)
                .DiscoverServices()
                .AddSignalRServer();

            return services;
        }

        internal IServiceCollection AddComponents()
        {
            services
                .AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            return services;
        }

        internal IServiceCollection AddAuth(IConfiguration configuration)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IApplicationPolicyProvider, AppPolicyProvider>();
            services.AddCascadingAuthenticationState();
            services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            authBuilder.AddOidcProviders(configuration);

            services.ConfigureApplicationCookie(config =>
            {
                config.ExpireTimeSpan = TimeSpan.FromDays(90);
                config.SlidingExpiration = true;

                config.Cookie.Name = "Mkx.Templates_Auth_Cookie";

                var defaultEvents = config.Events;

                config.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api"))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                        return defaultEvents.OnRedirectToLogin(ctx);
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api"))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                        return defaultEvents.OnRedirectToAccessDenied(ctx);
                    }
                };
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureOptions<ConfigureSecurityStampOptions>();

            return services;
        }

        internal IServiceCollection AddAuthOptions(IConfiguration configuration)
        {
            var loginSection = configuration.GetSection("Authentication:LoginOptions");

            services.Configure<LoginOptions>(loginSection);

            return services;
        }

        internal IServiceCollection AddOidcProviderOptions(IConfiguration configuration)
        {
            services.Configure<List<OidcDescriptor>>(
                configuration.GetSection("Authentication:ExternalProviders"));

            return services;
        }

        internal IServiceCollection AddSignalRServer()
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true; // Enable for debugging
                options.MaximumReceiveMessageSize = 102400;
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });

            return services;
        }
    }
}