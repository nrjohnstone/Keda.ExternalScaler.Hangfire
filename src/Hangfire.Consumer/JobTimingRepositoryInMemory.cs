using System;
using Hangfire.Server;
using Serilog;

namespace Hangfire.Consumer
{
    public class JobTimingRepositoryInMemory : IJobTimingRepository
    {
        public DateTimeOffset JobStarted { get; set; }
        public DateTimeOffset JobStopped { get; set; }
        public bool IsActive { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            JobStarted = DateTimeOffset.UtcNow;
            JobStopped = DateTimeOffset.UtcNow;
            IsInitialized = true;
        }

        public void RecordStarted(PerformingContext context)
        {
            JobStarted = DateTimeOffset.UtcNow;
            IsActive = true;
            Log.Information("Job recorded as started");
        }

        public void RecordStopped(PerformedContext context)
        {
            JobStopped = DateTimeOffset.UtcNow;
            IsActive = false;
            Log.Information("Job recorded as stopped");
        }

        public TimeSpan IdleTime
        {
            get
            {
                if (IsActive)
                {
                    Log.Information("Job is active. Idle time reported at 0");
                    return TimeSpan.FromMilliseconds(0);
                }

                return DateTimeOffset.UtcNow - JobStopped;
            }
        }
    }
}