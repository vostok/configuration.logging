using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Configuration.Logging.Tests
{
    [TestFixture]
    internal class ConfigurationProviderSettingsExtensions_Tests
    {
        private ConfigurationProviderSettings settings;
        private ILog log;
        private LogEvent observedEvent;

        [SetUp]
        public void TestSetup()
        {
            settings = new ConfigurationProviderSettings();

            observedEvent = null;

            log = Substitute.For<ILog>();
            log.IsEnabledFor(Arg.Any<LogLevel>()).Returns(true);
            log.ForContext(Arg.Any<string>()).Returns(_ => log);
            log.When(l => l.Log(Arg.Any<LogEvent>())).Do(info => observedEvent = info.Arg<LogEvent>());
        }

        [Test]
        public void WithErrorLogging_should_set_error_callback_if_there_is_none()
        {
            settings = settings.WithErrorLogging(log);

            settings.ErrorCallback.Should().NotBeNull();

            var error = new Exception("I failed..");

            settings.ErrorCallback?.Invoke(error);

            observedEvent.Level.Should().Be(LogLevel.Error);
            observedEvent.Exception.Should().BeSameAs(error);
        }

        [Test]
        public void WithErrorLogging_should_enrich_error_callback_if_there_is_one()
        {
            var originalCallback = Substitute.For<Action<Exception>>();

            settings.ErrorCallback = originalCallback;

            settings = settings.WithErrorLogging(log);

            settings.ErrorCallback.Should().NotBeNull();

            var error = new Exception("I failed..");

            settings.ErrorCallback?.Invoke(error);

            observedEvent.Level.Should().Be(LogLevel.Error);
            observedEvent.Exception.Should().BeSameAs(error);

            originalCallback.Received(1).Invoke(error);
        }

        [Test]
        public void WithSettingsLogging_should_set_settings_callback_if_there_is_none()
        {
            settings = settings.WithSettingsLogging(log);

            settings.SettingsCallback.Should().NotBeNull();

            settings.SettingsCallback?.Invoke(new object(), Substitute.For<IConfigurationSource>());

            observedEvent.Level.Should().Be(LogLevel.Info);
        }

        [Test]
        public void WithSettingsLogging_should_enrich_settings_callback_if_there_is_one()
        {
            var originalCallback = Substitute.For<Action<object, IConfigurationSource>>();

            settings.SettingsCallback = originalCallback;

            settings = settings.WithSettingsLogging(log);

            settings.SettingsCallback.Should().NotBeNull();

            settings.SettingsCallback?.Invoke(new object(), Substitute.For<IConfigurationSource>());

            observedEvent.Level.Should().Be(LogLevel.Info);

            originalCallback.Received(1).Invoke(Arg.Any<object>(), Arg.Any<IConfigurationSource>());
        }
    }
}