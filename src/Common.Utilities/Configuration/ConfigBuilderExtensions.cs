using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;

namespace Common.Utilities.Configuration
{
    public static class ConfigBuilderExtensions
    {
        /// <summary>
        /// Allow for overriding values when running locally on a developer machine by using configuration stored
        /// outside the git repo which prevents secrets from being committed etc...
        /// Will check for an optional "settings.yaml" and "settings.ini" file
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddOptionalLocalDevelopment(this IConfigurationBuilder builder, string applicationName)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var settingsFolder = Path.Combine(localAppData, "Keda.ExternalScaler.Hangfire",  applicationName);

            var developmentYaml = Path.Combine(settingsFolder, "settings.yaml");
            var developmentIni = Path.Combine(settingsFolder, "settings.ini");

            builder.AddIniFile(developmentIni, optional: true);
            builder.AddYamlFile(developmentYaml, optional: true);

            return builder;
        }

        /// <summary>
        /// Add default values from settings.ini in the root of the application
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddDefaults(this IConfigurationBuilder builder)
        {
            builder.AddIniFile("settings.ini", optional: true);
            builder.AddYamlFile("settings.yaml", optional: true);

            return builder;
        }
    }
}