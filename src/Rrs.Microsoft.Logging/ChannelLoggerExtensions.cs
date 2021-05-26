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
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ChannelLoggerOptions>, ChannelLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<ChannelLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ChannelLoggerOptions, ChannelLoggerProvider>>());
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
    }
}
