using System;
using SimpleInjector;

namespace Hangfire.Consumer
{
    public class SimpleInjectorJobActivatorScope : JobActivatorScope
    {
        private readonly Container _container;
        private readonly Scope _scope;
        private readonly JobActivatorContext _context;

        public SimpleInjectorJobActivatorScope(Container container, Scope scope, JobActivatorContext context)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (context == null) throw new ArgumentNullException(nameof(context));
            _container = container;
            _scope = scope;
            _context = context;
        }

        public override object Resolve(Type type)
        {
            var instance = _container.GetInstance(type);

            if (instance is IHangfireJob hangfireJob)
            {
                hangfireJob.SetJobId(_context.BackgroundJob.Id);
            }

            return instance;
        }

        public override void DisposeScope()
        {
            _scope.Dispose();
        }
    }
}