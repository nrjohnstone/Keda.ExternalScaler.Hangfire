namespace Hangfire.Consumer
{
    public interface IHangfireJob
    {
        void SetJobId(string jobId);
    }
}