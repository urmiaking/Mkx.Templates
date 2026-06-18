namespace Mkx.Templates.Sdk.Server.Infrastructure.Abstractions;

public interface IDbSeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken cancellationToken = default);
}
