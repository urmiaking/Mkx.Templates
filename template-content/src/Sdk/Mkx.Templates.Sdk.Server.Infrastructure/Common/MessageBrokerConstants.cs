namespace Mkx.Templates.Sdk.Server.Infrastructure.Common;

public static class MessageBrokerConstants
{
    public const string SubscriberQueueName = "BusSubscriberEventQueue";
    public const string PublisherQueueName = "BusPublisherEventQueue";
    public const string SubscriptionTableName = "BusSubscriptions";
    public const string TimeoutTableName = "BusTimeouts";
    public const string ErrorTableName = "BusErrors";
}
