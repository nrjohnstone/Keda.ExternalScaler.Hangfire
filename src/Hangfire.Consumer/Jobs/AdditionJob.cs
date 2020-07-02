using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Consumer.Contracts;
using Serilog;

namespace Hangfire.Consumer.Jobs
{
    internal class AdditionJob : IAdditionJob
    {
        public JobResult Execute(AdditionRequest request, CancellationToken token)
        {
            Log.Information("Starting addition job");

            int sum = request.Terms.First();

            foreach (var requestTerm in request.Terms.Skip(1))
            {
                sum += requestTerm;
            }

            Log.Information("Processing: {CalculationType} : {Result}", "addition", sum);
            
            Task.Delay(TimeSpan.FromSeconds(sum), token).GetAwaiter().GetResult();

            Log.Information("Finished addition job");

            return new JobResult()
            {
                Status = "Successful",
                Message = $"Sum: {sum}"
            };
        }
    }
}