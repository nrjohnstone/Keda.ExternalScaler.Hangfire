namespace HangfireExternalScaler
{
    public interface IHangfireMetricsApi
    {
        bool Exists(string hangfireInstanceName);
        long EnqueuedCount(string hangfireInstanceName, string queueName);
        long FetchedCount(string hangfireInstanceName, string queueName);
    }
}