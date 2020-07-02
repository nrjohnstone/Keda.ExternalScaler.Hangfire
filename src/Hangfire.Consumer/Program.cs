using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Common.Utilities.Configuration;
using Hangfire.Consumer.Configuration;
using Hangfire.Consumer.Contracts;
using Hangfire.Consumer.Jobs;
using Hangfire.SqlServer;
using Serilog;
using SimpleInjector;

namespace Hangfire.Consumer
{
    class Program
    {
        public const string ApplicationName = "Hangfire.Consumer";

        static void Main(string[] args)
        {
            var applicationConfiguration = new ApplicationConfiguration(args, ApplicationName).CreateBuilder().Build();

            ISettings settings = applicationConfiguration.ToSettings<Settings>(ApplicationName);

            Log.Logger = SerilogConfiguration.Create(ApplicationName, settings.MinimumLogLevel, settings.HttpLogEndpoint);

            Container container = new Container();

            // Register all types of jobs, but the HangfireQueue setting will control
            // which job type actually gets run because each job maps to a different queue
            // and Hangfire.Consumer only attaches to a single queue
            container.Register<IMultiplyJob, MultiplyJob>();
            container.Register<IAdditionJob, AdditionJob>();

            IJobTimingRepository jobRepository = new JobTimingRepositoryInMemory();

            ConfigureHangfire(settings, jobRepository);

            var serverJobOptions = new BackgroundJobServerOptions()
            {
                Queues = new[] { settings.HangfireQueue },
                WorkerCount = 1,
                HeartbeatInterval = TimeSpan.FromSeconds(30)
            };
            serverJobOptions.UseSimpleInjector(container);

            var server = new IdleBackgroundJobServer(jobRepository, serverJobOptions,
                TimeSpan.FromSeconds(settings.IdleTimeoutSeconds));

            bool shutdown = false;

            while (!shutdown)
            {
                shutdown = server.WaitForShutdown(TimeSpan.FromSeconds(5));
            }

            Log.CloseAndFlush();
        }

        private static void ConfigureHangfire(ISettings settings, IJobTimingRepository jobTimingRepository)
        {
            SqlServerStorageOptions options = new SqlServerStorageOptions();

            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
            csb.DataSource = settings.Hangfire.Address;
            csb.UserID = settings.Hangfire.Username;
            csb.Password = settings.Hangfire.Password;

            GlobalConfiguration.Configuration.UseSqlServerStorage(csb.ToString(), options);
            GlobalConfiguration.Configuration.UseFilter(new JobTimingReporter(jobTimingRepository));
        }
    }
}
