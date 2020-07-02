using System;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Hangfire.Consumer
{
    public class SimpleInjectorContainerJobActivator : JobActivator
    {
        private readonly Container _container;

        public SimpleInjectorContainerJobActivator(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            var scope = AsyncScopedLifestyle.BeginScope(_container);
            return new SimpleInjectorJobActivatorScope(_container, scope, context);
        }

        public override object ActivateJob(Type type)
        {
            var activateJob = _container.GetInstance(type);
            return activateJob;
        }
    }
}