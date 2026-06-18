using Mkx.Templates.Sdk.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Ui.Core.Extensions;
using Serilog.Ui.MsSqlServerProvider.Extensions;
using Serilog.Ui.Web.Extensions;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mkx.Templates.Sdk.Server.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddControllers(IConfiguration configuration)
        {
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            return services;
        }

        public IServiceCollection AddSwagger()
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        public IServiceCollection AddCache()
        {
            services.AddMemoryCache();

            return services;
        }

        public IServiceCollection AddSerilogUiService(IConfiguration configuration)
        {
            services.AddSerilogUi(opts =>
            {
                opts.UseSqlServer(options =>
                {
                    options
                        .WithConnectionString(configuration.GetConnectionString("Default")!)
                        .WithTable("Logs");
                });
            });

            return services;
        }

        public IServiceCollection InitializeDefaultCulture()
        {
            var culture = CultureHelper.GetPersianCulture();

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            return services;
        }
    }
}
