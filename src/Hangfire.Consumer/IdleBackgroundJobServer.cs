using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Server;

namespace Hangfire.Consumer
{
    public class IdleBackgroundJobServer : IBackgroundProcessingServer
    {
        private readonly BackgroundJobServer _server;

        public IdleBackgroundJobServer(
            IJobTimingRepository repository, 
            BackgroundJobServerOptions options,
            TimeSpan idleTimeout)
        {
            _server = new BackgroundJobServer(
                options,
                JobStorage.Current,
                new[] { new JobIdleWatcher(repository, this, idleTimeout) });
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public void SendStop()
        {
            _server.SendStop();
        }

        public bool WaitForShutdown(TimeSpan timeout)
        {
            return _server.WaitForShutdown(timeout);
        }

        public Task WaitForShutdownAsync(CancellationToken cancellationToken)
        {
            return _server.WaitForShutdownAsync(cancellationToken);
        }
    }
}