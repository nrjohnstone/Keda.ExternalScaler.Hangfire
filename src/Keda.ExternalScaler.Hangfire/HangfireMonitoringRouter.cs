using System.Collections.Generic;
using Hangfire.Storage;

namespace HangfireExternalScaler
{
    /// <summary>
    /// Provide keyed access to a collection of Hangfire MonitoringApi instances
    /// for handling multiple hangfire instances with a single Hangfire External Scaler
    /// </summary>
    public class HangfireMonitoringRouter : IHangfireMetricsApi
    {
        private readonly Dictionary<string, IMonitoringApi> _monitoringApis;

        public HangfireMonitoringRouter()
        {
            _monitoringApis = new Dictionary<string, IMonitoringApi>();
        }

        public bool Exists(string hangfireInstanceName)
        {
            return _monitoringApis.ContainsKey(hangfireInstanceName);
        }
        
        public void AddMonitoringApi(string hangfireInstanceName, IMonitoringApi monitoringApi)
        {
            _monitoringApis.Add(hangfireInstanceName, monitoringApi);
        }

        public long EnqueuedCount(string hangfireInstanceName, string queueName)
        {
            return _monitoringApis[hangfireInstanceName].EnqueuedCount(queueName);
        }

        public long FetchedCount(string hangfireInstanceName, string queueName)
        {
            return _monitoringApis[hangfireInstanceName].FetchedCount(queueName);
        }
    }
}