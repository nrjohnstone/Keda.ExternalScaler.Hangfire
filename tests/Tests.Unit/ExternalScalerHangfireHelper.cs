using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Storage;
using Keda.ExternalScaler.Hangfire;
using Keda.ExternalScaler.Hangfire.Configuration;
using Microsoft.Extensions.Hosting;

namespace Tests.Unit
{
    internal class ExternalScalerHangfireHelper
    {
        public HangfireScaledObjectRepositoryInMemory ScaledObjectRepository { get; private set; }
        public IList<HangfireMonitorTestDouble> MonitoringApis { get; private set; }

        private readonly ISettings _settings;
        private Task _hostTask;
        private IHost _host;
        

        public ExternalScalerHangfireHelper(ISettings settings)
        {
            _settings = settings;
            ScaledObjectRepository = new HangfireScaledObjectRepositoryInMemory();
            MonitoringApis = new List<HangfireMonitorTestDouble>();
        }

        public void Run()
        {
            ApplicationHostBuilderTestDouble appHostBuilder = new ApplicationHostBuilderTestDouble(new string[] { },
                _settings, ScaledObjectRepository, MonitoringApis);

            _host = appHostBuilder.Create().Build();
            _hostTask = Task.Run(() => _host.Run());
            
            Thread.Sleep(1000);
        }

        public void Stop()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _hostTask.Wait();
        }
    }
}