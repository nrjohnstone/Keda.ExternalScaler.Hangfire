using System.Collections.Generic;
using System.Data.SqlClient;
using Hangfire.SqlServer;
using Hangfire.Storage;
using HangfireExternalScaler.Configuration;
using HangfireExternalScaler.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Context;

namespace HangfireExternalScaler
{
    public class ApplicationHostBuilder
    {
        private readonly string[] _args;
        private ISettings _settings;

        public ApplicationHostBuilder(string[] args, ISettings settings)
        {
            _args = args;
            _settings = settings;
        }

        public virtual IHostBuilder Create()
        {
            return Host.CreateDefaultBuilder(_args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel()
                        .ConfigureServices(services => ConfigureServices(services))
                        .Configure( (context, app) => Configure(app, context));
                        
                });
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var hangfireMonitoringRouter = ConfigureHangfireMonitoring(_settings.HangfireSqlInstances);

            services.AddSingleton<IHangfireMetricsApi>(hangfireMonitoringRouter);
            services.AddGrpc();
        }

        private IHangfireMetricsApi ConfigureHangfireMonitoring(IEnumerable<HangfireSqlServerSettings> hangfireSqlServerInstance)
        {
            HangfireMonitoringRouter hangfireMonitoringRouter = new HangfireMonitoringRouter();

            foreach (var hangfireSettings in hangfireSqlServerInstance)
            {
                IMonitoringApi monitoringApi = CreateHangfireMonitor(hangfireSettings);
                hangfireMonitoringRouter.AddMonitoringApi(hangfireSettings.Name, monitoringApi);
            }

            return hangfireMonitoringRouter;
        }

        protected virtual IMonitoringApi CreateHangfireMonitor(HangfireSqlServerSettings sqlServerSettings)
        {
            using (LogContext.PushProperty("HangfireName", sqlServerSettings.Name))
            {
                SqlServerStorageOptions options = new SqlServerStorageOptions();

                SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
                csb.DataSource = sqlServerSettings.Address;
                csb.UserID = sqlServerSettings.Username;
                csb.Password = sqlServerSettings.Password;

                var storage = new SqlServerStorage(csb.ToString(), options);
                
                var monitoringApi = storage.GetMonitoringApi();
                return monitoringApi;
            }
        }

        private void Configure(IApplicationBuilder app, WebHostBuilderContext webHostBuilderContext)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ExternalScalerService>();
                
                endpoints.MapGet("/", async context =>
                {
               
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}