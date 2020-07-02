using System;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Dashboard.Configuration;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hangfire.Dashboard
{
    public class ApplicationHostBuilder
    {
        private readonly string _applicationName;
        private readonly string[] _args;
        private readonly IConfiguration _configuration;
        private readonly ISettings _settings;

        public ApplicationHostBuilder(string[] args, string applicationName, IConfiguration configuration,
            ISettings settings)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(applicationName));
            _applicationName = applicationName;
            _args = args;
            _configuration = configuration;
            _settings = settings;
        }

        public IHostBuilder Create()
        {
            return Host.CreateDefaultBuilder(_args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseConfiguration(_configuration)
                        .ConfigureServices(services => ConfigureServices(services))
                        .Configure(app => Configure(app));
                });
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            
            ConfigureHangfire(services);
        }

        private void ConfigureHangfire(IServiceCollection services)
        {
            SqlServerStorageOptions options = new SqlServerStorageOptions();
            options.QueuePollInterval = TimeSpan.FromSeconds(2);

            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
            csb.DataSource = _settings.HangfireSqlInstances.First().Address;
            csb.UserID = _settings.HangfireSqlInstances.First().Username;
            csb.Password = _settings.HangfireSqlInstances.First().Password;

            var storage = new SqlServerStorage(csb.ToString(), options);

            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(csb.ToString(), options);
            });
         
            IBackgroundJobClient client = new BackgroundJobClient(storage);
            IMonitoringApi monitoringApi = storage.GetMonitoringApi();
            
            services.AddSingleton(client);
            services.AddSingleton(monitoringApi);
        }

        private void Configure(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            var env = serviceProvider.GetService<IWebHostEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AppPath = "/",
                IgnoreAntiforgeryToken = true,
                Authorization = new[] { new DashboardNoAuthorizationFilter() }
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
            Log.Information("Dashboard started");
        }
    }
}