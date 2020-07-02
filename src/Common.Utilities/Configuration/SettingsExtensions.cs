using System;
using Microsoft.Extensions.Configuration;

namespace Common.Utilities.Configuration
{
    public static class SettingsExtensions
    {
        public static TSettings ToSettings<TSettings>(this IConfiguration configuration, string applicationName)
        {
            var settings = Activator.CreateInstance<TSettings>();

            configuration.Bind(settings);

            return settings;
        }
    }
}