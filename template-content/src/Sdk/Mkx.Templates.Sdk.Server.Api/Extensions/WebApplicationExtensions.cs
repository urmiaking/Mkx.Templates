using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Mkx.Templates.Sdk.Server.Api.Extensions;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication ConfigureSqlSerilog()
        {
            using var scope = app.Services.CreateScope();

            // Reconfigure Serilog with MSSqlServer sink
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            loggerFactory.AddSerilog(Log.Logger);

            return app;
        }
    }
}
