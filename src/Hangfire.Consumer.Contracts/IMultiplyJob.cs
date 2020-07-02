using System.Collections.Generic;
using System.Threading;

namespace Hangfire.Consumer.Contracts
{
    [Queue("multiply")]
    public interface IMultiplyJob
    {
        JobResult Execute(MultiplyRequest request, CancellationToken token);
    }

    public class MultiplyRequest
    {
        public IEnumerable<int> Factors { get; set; }
    }

    
}
