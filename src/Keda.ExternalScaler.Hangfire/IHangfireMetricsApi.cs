namespace Keda.ExternalScaler.Hangfire
{
    public interface IHangfireMetricsApi
    {
        long EnqueuedCount(string hangfireInstanceName, string queueName);
        long FetchedCount(string hangfireInstanceName, string queueName);
    }
}