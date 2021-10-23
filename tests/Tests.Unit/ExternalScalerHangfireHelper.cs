using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HangfireExternalScaler.Configuration;
using Microsoft.Extensions.Hosting;

namespace HangfireExternalScaler.Tests.Unit
{
    internal class ExternalScalerHangfireHelper
    {
        public IList<HangfireMonitorTestDouble> MonitoringApis { get; private set; }

        private readonly ISettings _settings;
        private Task _hostTask;
        private IHost _host;
        

        public ExternalScalerHangfireHelper(ISettings settings)
        {
            _settings = settings;
            MonitoringApis = new List<HangfireMonitorTestDouble>();
        }

        public void Run()
        {
            ApplicationHostBuilderTestDouble appHostBuilder = new ApplicationHostBuilderTestDouble(new string[] { },
                _settings, MonitoringApis);

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