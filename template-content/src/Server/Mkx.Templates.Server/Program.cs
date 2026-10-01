using Mkx.Templates.Server.Extensions;
using Mkx.Templates.Infrastructure;
using Mkx.Templates.Sdk.Server.Api.Extensions;
using Mkx.Templates.Sdk.Server.Infrastructure.Extensions;

try
{
    Console.WriteLine("Application is starting up.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.ConfigureSerilog();
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

    var app = builder.ConfigureServices();

    if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        app.ApplyDatabaseMigrations<AppDbContext>();

    if (app.Configuration.GetValue<bool>("Database:SeedOnStartup"))
        await app.SeedDatabaseAsync();

    if (app.Configuration.GetValue<bool>("Logging:UseSqlStore"))
        app.ConfigureSqlSerilog();

    app.ConfigurePipeline();

    app.Run();
}

catch (Exception ex) when (ex is not HostAbortedException)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("An error occurred:");
    Console.WriteLine(ex);
    throw;
}
finally
{
    Console.ResetColor();
    Console.WriteLine("Shutting down completed.");
}

public partial class Program { }
