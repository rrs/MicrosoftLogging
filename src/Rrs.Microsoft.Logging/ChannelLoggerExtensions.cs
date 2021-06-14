using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Rrs.Microsoft.Logging
{
    public static class ChannelLoggerExtensions
    {
        /// <summary>
        /// Adds the file logger provider, aliased as 'File', in the available services as singleton and binds the file logger options class to the 'File' section of the appsettings.json file.
        /// </summary>
        public static ILoggingBuilder AddChannelLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ChannelLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<ChannelLoggerOptions, ChannelLoggerProvider>(builder.Services);
            //builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ChannelLoggerOptions>, ChannelLoggerOptionsSetup>());
            //builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<ChannelLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ChannelLoggerOptions, ChannelLoggerProvider>>());
            builder.Services.AddSingleton<LoggerChannel>();
            return builder;
        }
        /// <summary>
        /// Adds the file logger provider, aliased as 'File', in the available services as singleton and binds the file logger options class to the 'File' section of the appsettings.json file.
        /// </summary>
        public static ILoggingBuilder AddChannelLogger(this ILoggingBuilder builder, Action<ChannelLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddChannelLogger();
            builder.Services.Configure(configure);

            return builder;
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        public static ILoggerFactory AddChannelLogger(this ILoggerFactory factory, LoggerChannel channel)
        {
            return factory.AddChannelLogger(includeScopes: false, channel);
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddChannelLogger(this ILoggerFactory factory, bool includeScopes, LoggerChannel channel)
        {
            factory.AddChannelLogger((n, l) => l >= LogLevel.Information, includeScopes, channel);
            return factory;
        }


        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        public static ILoggerFactory AddChannelLogger(this ILoggerFactory factory, LogLevel minLevel, LoggerChannel channel)
        {
            factory.AddChannelLogger(minLevel, includeScopes: false, channel);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddChannelLogger(
            this ILoggerFactory factory,
            LogLevel minLevel,
            bool includeScopes,
            LoggerChannel channel)
        {
            factory.AddChannelLogger((category, logLevel) => logLevel >= minLevel, includeScopes, channel);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="filter">The category filter to apply to logs.</param>
        public static ILoggerFactory AddChannelLogger(
            this ILoggerFactory factory,
            Func<string, LogLevel, bool> filter,
            LoggerChannel channel)
        {
            factory.AddChannelLogger(filter, includeScopes: false, channel);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="filter">The category filter to apply to logs.</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddChannelLogger(
            this ILoggerFactory factory,
            Func<string, LogLevel, bool> filter,
            bool includeScopes,
            LoggerChannel channel)
        {
            factory.AddProvider(new ChannelLoggerProvider(filter, includeScopes, channel));
            return factory;
        }


        /// <summary>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="settings">The settings to apply to created <see cref="ConsoleLogger"/>'s.</param>
        /// <returns></returns>
        public static ILoggerFactory AddChannelLogger(
            this ILoggerFactory factory,
            IChannelLoggerSettings settings,
            LoggerChannel channel)
        {
            factory.AddProvider(new ChannelLoggerProvider(settings, channel));
            return factory;
        }

        /// <summary>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use for <see cref="IConsoleLoggerSettings"/>.</param>
        /// <returns></returns>
        public static ILoggerFactory AddChannelLogger(this ILoggerFactory factory, IConfiguration configuration, LoggerChannel channel)
        {
            var settings = new ConfigurationChannelLoggerSettings(configuration);
            return factory.AddChannelLogger(settings, channel);
        }
    }
}
