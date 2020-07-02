using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Common.Utilities.Configuration;
using Hangfire.Consumer.Contracts;
using Hangfire.Producer.Configuration;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Serilog;

namespace Hangfire.Producer
{
    class Program
    {
        public const string ApplicationName = "Hangfire.Producer";

        static void Main(string[] args)
        {
            var applicationConfiguration = new ApplicationConfiguration(args, ApplicationName).CreateBuilder().Build();

            ISettings settings = applicationConfiguration.ToSettings<Settings>(ApplicationName);

            Log.Logger = SerilogConfiguration.Create(ApplicationName, settings.MinimumLogLevel,
                settings.HttpLogEndpoint);

            ConfigureHangfire(settings);

            var client = new BackgroundJobClient(JobStorage.Current);

            if (settings.RequestType.Equals("addition"))
            {
                CreateAdditionJobs(settings, client);
            }
            else if (settings.RequestType.Equals("multiply"))
            {
                CreateMultiplyJobs(settings, client);
            }

        }

        private static void CreateMultiplyJobs(ISettings settings, BackgroundJobClient client)
        {
            Random rand = new Random();

            for (int i = 0; i < settings.NumberOfJobs; i++)
            {
                int term1 = rand.Next(1, 12);

                MultiplyRequest request = new MultiplyRequest()
                {
                     Factors = new List<int>() { term1, 10}
                };

                string jobId = client.Enqueue<IMultiplyJob>(x => x.Execute(request, CancellationToken.None));
                Log.Logger.Information("Multiply job created with JobId: {JobId}", jobId);
            }
        }

        private static void CreateAdditionJobs(ISettings settings, BackgroundJobClient client)
        {
            Random rand = new Random();

            for (int i = 0; i < settings.NumberOfJobs; i++)
            {
                int term1 = rand.Next(10, 60);
                int term2 = rand.Next(10, 60);

                AdditionRequest additionRequest = new AdditionRequest()
                {
                    Terms = new List<int>() { term1, term2 }
                };

                string jobId = client.Enqueue<IAdditionJob>(x => x.Execute(additionRequest, CancellationToken.None));
                Log.Logger.Information("Addition job created with JobId: {JobId}", jobId);
            }
        }

        private static void ConfigureHangfire(ISettings settings)
        {
            SqlServerStorageOptions options = new SqlServerStorageOptions();

            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
            csb.DataSource = settings.HangfireInstances.First().Address;
            csb.UserID = settings.HangfireInstances.First().Username;
            csb.Password = settings.HangfireInstances.First().Password;

            GlobalConfiguration.Configuration.UseSqlServerStorage(csb.ToString(), options);
        }

    }
}
