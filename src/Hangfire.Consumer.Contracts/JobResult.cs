namespace Hangfire.Consumer.Contracts
{
    public class JobResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}