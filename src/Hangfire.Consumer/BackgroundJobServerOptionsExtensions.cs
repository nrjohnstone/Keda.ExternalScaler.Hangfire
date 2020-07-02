using SimpleInjector;

namespace Hangfire.Consumer
{
    public static class BackgroundJobServerOptionsExtensions
    {
        public static void UseSimpleInjector(this BackgroundJobServerOptions options, Container container)
        {
            options.Activator = new SimpleInjectorContainerJobActivator(container);
        }
    }
}