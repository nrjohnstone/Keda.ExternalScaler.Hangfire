using Hangfire.Server;

namespace Hangfire.Consumer
{
    /// <summary>
    /// Record the datetime when a job starts and finishes
    /// </summary>
    public sealed class JobTimingReporter : IServerFilter
    {
        private readonly IJobTimingRepository _repository;

        public JobTimingReporter(IJobTimingRepository repository)
        {
            _repository = repository;
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            _repository.RecordStarted(filterContext);
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            _repository.RecordStopped(filterContext);
        }
    }
}