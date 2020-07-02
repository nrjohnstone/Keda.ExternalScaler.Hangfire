using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Common.Utilities.Configuration
{
    public class ApplicationConfiguration
    {
        private readonly string[] _args;
        private readonly string _applicationName;

        public ApplicationConfiguration(string[] args, string applicationName)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (applicationName == null) throw new ArgumentNullException(nameof(applicationName));

            _args = args;
            _applicationName = applicationName;
        }

        public IConfigurationBuilder CreateBuilder()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            configurationBuilder
                .AddDefaults()
                .AddOptionalLocalDevelopment(_applicationName)
                .AddEnvironmentVariables()
                .AddCommandLine(_args);

            return configurationBuilder;
        }
    }
}