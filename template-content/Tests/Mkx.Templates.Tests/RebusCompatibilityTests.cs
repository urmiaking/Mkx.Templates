using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Transport.InMem;

namespace Mkx.Templates.Tests;

public sealed class RebusCompatibilityTests
{
    [Fact]
    public async Task RebusStartsWithDotNet11DependencyInjection()
    {
        using var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddRebus(configure => configure
                    .Transport(transport => transport.UseInMemoryTransport(
                        new InMemNetwork(), "mkx-compatibility-test"))
                    .Options(options => options.SetNumberOfWorkers(0)));
            })
            .Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            Assert.NotNull(host.Services.GetRequiredService<IBus>());
        }
        finally
        {
            await host.StopAsync(TestContext.Current.CancellationToken);
        }
    }
}