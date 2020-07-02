using System.Collections.Generic;
using System.Threading;

namespace Hangfire.Consumer.Contracts
{
    [Queue("addition")]
    public interface IAdditionJob
    {
        JobResult Execute(AdditionRequest request, CancellationToken token);
    }

    public class AdditionRequest
    {
        public IEnumerable<int> Terms { get; set; }
    }
}