using System;
using System.Collections.Generic;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace HangfireExternalScaler.Tests.Unit
{
    /// <summary>
    /// Test Double implementation for the Hangfire IMonitoringApi
    /// </summary>
    internal class HangfireMonitorTestDouble : IMonitoringApi
    {
        public string Name { get; }

        public IList<QueueWithTopEnqueuedJobsDto> Queues()
        {
            return null;
        }

        public IList<ServerDto> Servers()
        {
            return null;
        }

        public JobDetailsDto JobDetails(string jobId)
        {
            return null;
        }

        public StatisticsDto GetStatistics()
        {
            return null;
        }

        public JobList<EnqueuedJobDto> EnqueuedJobs(string queue, int @from, int perPage)
        {
            return null;
        }

        public JobList<FetchedJobDto> FetchedJobs(string queue, int @from, int perPage)
        {
            return null;
        }

        public JobList<ProcessingJobDto> ProcessingJobs(int @from, int count)
        {
            return null;
        }

        public JobList<ScheduledJobDto> ScheduledJobs(int @from, int count)
        {
            return null;
        }

        public JobList<SucceededJobDto> SucceededJobs(int @from, int count)
        {
            return null;
        }

        public JobList<FailedJobDto> FailedJobs(int @from, int count)
        {
            return null;
        }

        public JobList<DeletedJobDto> DeletedJobs(int @from, int count)
        {
            return null;
        }

        public long ScheduledCount()
        {
            return 0;
        }

        public Dictionary<string, long> EnqueuedCounts = new Dictionary<string, long>();
        public Dictionary<string, long> FetchedCounts = new Dictionary<string, long>();

        public HangfireMonitorTestDouble(string name)
        {
            Name = name;
        }

        public long EnqueuedCount(string queue)
        {
            if (EnqueuedCounts.ContainsKey(queue))
            {
                return EnqueuedCounts[queue];
            }

            return 0;
        }

        public void SetEnqueuedCount(string queue, long count)
        {
            EnqueuedCounts[queue] = count;
        }

        public void SetFetchedCount(string queue, long count)
        {
            FetchedCounts[queue] = count;
        }

        public long FetchedCount(string queue)
        {
            if (FetchedCounts.ContainsKey(queue))
            {
                return FetchedCounts[queue];
            }

            return 0;
        }

        public long FailedCount()
        {
            return 0;
        }

        public long ProcessingCount()
        {
            return 0;
        }

        public long SucceededListCount()
        {
            return 0;
        }

        public long DeletedListCount()
        {
            return 0;
        }

        public IDictionary<DateTime, long> SucceededByDatesCount()
        {
            return null;
        }

        public IDictionary<DateTime, long> FailedByDatesCount()
        {
            return null;
        }

        public IDictionary<DateTime, long> HourlySucceededJobs()
        {
            return null;
        }

        public IDictionary<DateTime, long> HourlyFailedJobs()
        {
            return null;
        }

    }
}