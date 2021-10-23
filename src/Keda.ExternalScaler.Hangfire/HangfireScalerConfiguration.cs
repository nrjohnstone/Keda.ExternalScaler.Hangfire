namespace HangfireExternalScaler
{
    public class HangfireScalerConfiguration
    {
        public string Queue { get; set; }
        public long TargetSize { get; set; }
        public string InstanceName { get; set; }
    }
}