using System;
using Hangfire.Server;
using Serilog;

namespace Hangfire.Consumer
{
    /// <summary>
    /// Periodically check if the idle time has been exceeded and trigger
    /// the hangfire server to shutdown
    /// </summary>
    public class JobIdleWatcher : IBackgroundProcess
    {
        private readonly IJobTimingRepository _repository;
        private readonly IBackgroundProcessingServer _server;
        private readonly TimeSpan _idleTimeout;

        public JobIdleWatcher(
            IJobTimingRepository repository, 
            IBackgroundProcessingServer server,
            TimeSpan idleTimeout)
        {
            _repository = repository;
            _server = server;
            _idleTimeout = idleTimeout;
        }

        public void Execute(BackgroundProcessContext context)
        {
            if (!_repository.IsInitialized)
            {
                Log.Information("Initializing job timing repository");
                _repository.Initialize();
            }

            var repositoryIdleTime = _repository.IdleTime;
            Log.Information($"Repository idle time: {repositoryIdleTime.TotalSeconds}");
            if (repositoryIdleTime > _idleTimeout)
            {
                Log.Information("Idle time exceeded. Shutting down");
                _server.SendStop();
                return;
            }

            Log.Information("Idle time NOT exceeded. Waiting for 5 seconds");
            context.Wait(TimeSpan.FromSeconds(5));
        }
    }
}