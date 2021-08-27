using System;
using JetBrains.Annotations;
using Vostok.Configuration.Printing;
using Vostok.Logging.Abstractions;

namespace Vostok.Configuration.Logging
{
    [PublicAPI]
    public static class ConfigurationProviderSettingsExtensions
    {
        /// <inheritdoc cref="WithErrorLogging(ConfigurationProviderSettings,ILog)"/>
        public static ConfigurationProviderSettings WithErrorLogging([NotNull] this ConfigurationProviderSettings settings, [NotNull] Func<ILog> logProvider)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (logProvider == null)
                throw new ArgumentNullException(nameof(logProvider));

            var currentCallback = settings.ErrorCallback;

            settings.ErrorCallback = exception =>
            {
                currentCallback?.Invoke(exception);

                var log = logProvider();
                if (log == null)
                    throw new ArgumentNullException(nameof(log));

                log = log.ForContext<ConfigurationProvider>();

                log.Error(exception, "An error has occured in configuration provider.");
            };

            return settings;
        }

        /// <summary>
        /// Enriches <see cref="ConfigurationProviderSettings.ErrorCallback"/> of given <paramref name="settings"/> with exception logging.
        /// </summary>
        [NotNull]
        public static ConfigurationProviderSettings WithErrorLogging([NotNull] this ConfigurationProviderSettings settings, [NotNull] ILog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            return WithErrorLogging(settings, () => log);
        }

        /// <inheritdoc cref="WithSettingsLogging(ConfigurationProviderSettings,ILog,PrintSettings)"/>
        [NotNull]
        public static ConfigurationProviderSettings WithSettingsLogging([NotNull] this ConfigurationProviderSettings settings, [NotNull] Func<ILog> logProvider)
            => WithSettingsLogging(settings, logProvider, null);

        /// <inheritdoc cref="WithSettingsLogging(ConfigurationProviderSettings,ILog,PrintSettings)"/>
        [NotNull]
        public static ConfigurationProviderSettings WithSettingsLogging([NotNull] this ConfigurationProviderSettings settings, [NotNull] ILog log)
            => WithSettingsLogging(settings, log, null);

        /// <inheritdoc cref="WithSettingsLogging(ConfigurationProviderSettings,ILog,PrintSettings)"/>
        [NotNull]
        public static ConfigurationProviderSettings WithSettingsLogging(
            [NotNull] this ConfigurationProviderSettings settings,
            [NotNull] ILog log,
            [CanBeNull] PrintSettings printSettings)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            return WithSettingsLogging(settings, () => log, printSettings);
        }

        /// <summary>
        /// <para>Enriches <see cref="ConfigurationProviderSettings.SettingsCallback"/> of given <paramref name="settings"/> with logging of new settings instances.</para>
        /// <para>Values are rendered with <see cref="ConfigurationPrinter"/>.</para>
        /// </summary>
        [NotNull]
        public static ConfigurationProviderSettings WithSettingsLogging(
            [NotNull] this ConfigurationProviderSettings settings,
            [NotNull] Func<ILog> logProvider,
            [CanBeNull] PrintSettings printSettings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (logProvider == null)
                throw new ArgumentNullException(nameof(logProvider));

            var currentCallback = settings.SettingsCallback;

            settings.SettingsCallback = (value, source) =>
            {
                currentCallback?.Invoke(value, source);

                var log = logProvider();
                if (log == null)
                    throw new ArgumentNullException(nameof(log));

                log = log.ForContext<ConfigurationProvider>();

                log.Info(
                    "Initialized new settings of type '{SettingsType}' from source of type '{SourceType}': \n{SettingsObject}",
                    value?.GetType()?.Name,
                    source?.GetType()?.Name,
                    ConfigurationPrinter.Print(value, printSettings));
            };

            return settings;
        }
    }
}