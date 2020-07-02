using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Consumer.Contracts;
using Serilog;

namespace Hangfire.Consumer.Jobs
{
    internal class MultiplyJob : IMultiplyJob
    {
        public JobResult Execute(MultiplyRequest request, CancellationToken token)
        {
            Log.Information("Starting multiply job");

            int product = request.Factors.First();

            foreach (var requestFactor in request.Factors.Skip(1))
            {
                product *= requestFactor;
            }
            Log.Information("Processing: {CalculationType} : {Result}", "multiply", product);

            Task.Delay(TimeSpan.FromSeconds(10), token).GetAwaiter().GetResult();

            Log.Information("Finished multiply job");

            return new JobResult()
            {
                Status = "Successful",
                Message = $"Product: {product}"
            };
        }
    }
}