using Mkx.Templates.Sdk.Server.Infrastructure.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRebus(
        this IServiceCollection services,
        IConfiguration configuration,
        string inputQueueName,
        Action<StandardConfigurer<IRouter>>? routing = null,
        Func<IBus, Task>? onCreated = null)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddRebus(rebus =>
            {
                rebus
#pragma warning disable CS0618 // Type or member is obsolete
                    .Transport(t => t.UseSqlServer(connectionString, inputQueueName))
#pragma warning restore CS0618 // Type or member is obsolete
                    .Subscriptions(s => s.StoreInSqlServer(
                        connectionString,
                        MessageBrokerConstants.SubscriptionTableName))
                    .Timeouts(t => t.StoreInSqlServer(
                        connectionString,
                        MessageBrokerConstants.TimeoutTableName))
                    .Options(o =>
                    {
                        o.SetNumberOfWorkers(5);
                        o.SetMaxParallelism(10);
                        o.RetryStrategy(MessageBrokerConstants.ErrorTableName);
                    });

                if (routing != null)
                {
                    rebus.Routing(routing);
                }

                return rebus;
            },
            onCreated: onCreated,
            isDefaultBus: true);

        return services;
    }
}
