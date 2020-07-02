namespace Keda.ExternalScaler.Hangfire
{
    public class HangfireScalerConfiguration
    {
        public string Queue { get; set; }
        public long MaxScale { get; set; }
        public string InstanceName { get; set; }
    }
}