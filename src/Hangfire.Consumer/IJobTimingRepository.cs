using System;
using Hangfire.Server;

namespace Hangfire.Consumer
{
    public interface IJobTimingRepository
    {
        void RecordStarted(PerformingContext context);
        void RecordStopped(PerformedContext context);

        bool IsInitialized { get; }

        void Initialize();
        TimeSpan IdleTime { get; }
    }
}