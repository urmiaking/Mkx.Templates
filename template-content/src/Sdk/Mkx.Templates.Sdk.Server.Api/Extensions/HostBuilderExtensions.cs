using Microsoft.Extensions.Hosting;
using Serilog;

namespace Mkx.Templates.Sdk.Server.Api.Extensions;

public static class HostBuilderExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public void ConfigureSerilog()
        {
            hostBuilder.UseSerilog((_, configuration) =>
            {
                configuration
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
            });
        }
    }
}
