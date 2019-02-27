using System;
using JetBrains.Annotations;
using Vostok.Configuration.Printing;
using Vostok.Logging.Abstractions;

namespace Vostok.Configuration.Logging
{
    [PublicAPI]
    public static class ConfigurationProviderSettingsExtensions
    {
        /// <summary>
        /// Enriches <see cref="ConfigurationProviderSettings.ErrorCallback"/> of given <paramref name="settings"/> with exception logging.
        /// </summary>
        [NotNull]
        public static ConfigurationProviderSettings WithErrorLogging([NotNull] this ConfigurationProviderSettings settings, [NotNull] ILog log)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (log == null)
                throw new ArgumentNullException(nameof(log));

            log = log.ForContext<ConfigurationProvider>();

            var currentCallback = settings.ErrorCallback;

            settings.ErrorCallback = exception =>
            {
                currentCallback?.Invoke(exception);

                log.Error(exception, "An error has occured in configuration provider.");
            };

            return settings;
        }

        /// <summary>
        /// <para>Enriches <see cref="ConfigurationProviderSettings.SettingsCallback"/> of given <paramref name="settings"/> with logging of new settings instances.</para>
        /// <para>Values are rendered with <see cref="ConfigurationPrinter"/>.</para>
        /// </summary>
        [NotNull]
        public static ConfigurationProviderSettings WithSettingsLogging([NotNull] this ConfigurationProviderSettings settings, [NotNull] ILog log)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (log == null)
                throw new ArgumentNullException(nameof(log));

            log = log.ForContext<ConfigurationProvider>();

            var currentCallback = settings.SettingsCallback;

            settings.SettingsCallback = (value, source) =>
            {
                currentCallback?.Invoke(value, source);

                log.Info("Initialized new settings of type '{SettingsType}' from source of type '{SourceType}': \n{SettingsObject}",
                    value?.GetType()?.Name, source?.GetType()?.Name, ConfigurationPrinter.Print(value));
            };

            return settings;
        }
    }
}
