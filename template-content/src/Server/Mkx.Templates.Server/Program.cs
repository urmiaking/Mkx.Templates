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

    app.ApplyDatabaseMigrations<AppDbContext>();

    await app.SeedDatabaseAsync();

    app.ConfigureSqlSerilog();

    app.ConfigurePipeline();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("An error occurred:");
    Console.WriteLine(ex.Message);
}
finally
{
    Console.ResetColor();
    Console.WriteLine("Shutting down completed.");
}
