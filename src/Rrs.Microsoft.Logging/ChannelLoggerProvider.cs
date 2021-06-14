using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;

namespace Rrs.Microsoft.Logging
{
    [ProviderAlias("Channel")]
    public class ChannelLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, ChannelLogger> _loggers = new ConcurrentDictionary<string, ChannelLogger>();

        private readonly Func<string, LogLevel, bool> _filter;
        private IChannelLoggerSettings _settings;

        private static readonly Func<string, LogLevel, bool> trueFilter = (cat, level) => true;
        private static readonly Func<string, LogLevel, bool> falseFilter = (cat, level) => false;
        private IDisposable _optionsReloadToken;
        private bool _includeScopes;
        private IExternalScopeProvider _scopeProvider;
        private string _timestampFormat;
        private Channel<Log> _channel;


        public ChannelLoggerProvider(Func<string, LogLevel, bool> filter, bool includeScopes, LoggerChannel channel)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            _filter = filter;
            _includeScopes = includeScopes;
            _channel = channel.Channel;
        }

        public ChannelLoggerProvider(IOptionsMonitor<ChannelLoggerOptions> options, LoggerChannel channel)
        {
            // Filter would be applied on LoggerFactory level
            _filter = trueFilter;
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            _channel = channel.Channel;
            ReloadLoggerOptions(options.CurrentValue);
        }

        private void ReloadLoggerOptions(ChannelLoggerOptions options)
        {
            _includeScopes = options.IncludeScopes;
            _timestampFormat = options.TimestampFormat;
            var scopeProvider = GetScopeProvider();
            foreach (var logger in _loggers.Values)
            {
                logger.ScopeProvider = scopeProvider;
                logger.TimestampFormat = options.TimestampFormat;
            }
        }

        public ChannelLoggerProvider(IChannelLoggerSettings settings, LoggerChannel channel)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            _settings = settings;
            _channel = channel.Channel;

            if (_settings.ChangeToken != null)
            {
                _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        private void OnConfigurationReload(object state)
        {
            try
            {
                // The settings object needs to change here, because the old one is probably holding on
                // to an old change token.
                _settings = _settings.Reload();

                _includeScopes = _settings?.IncludeScopes ?? false;

                var scopeProvider = GetScopeProvider();
                foreach (var logger in _loggers.Values)
                {
                    logger.Filter = GetFilter(logger.Name, _settings);
                    logger.ScopeProvider = scopeProvider;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error while loading configuration changes.{Environment.NewLine}{ex}");
            }
            finally
            {
                // The token will change each time it reloads, so we need to register again.
                if (_settings?.ChangeToken != null)
                {
                    _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
                }
            }
        }

        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private ChannelLogger CreateLoggerImplementation(string name)
        {
            var includeScopes = _settings?.IncludeScopes ?? _includeScopes;

            return new ChannelLogger(name, GetFilter(name, _settings), includeScopes ? _scopeProvider : null, _channel)
            {
                TimestampFormat = _timestampFormat,
            };
        }

        private Func<string, LogLevel, bool> GetFilter(string name, IChannelLoggerSettings settings)
        {
            if (_filter != null)
            {
                return _filter;
            }

            if (settings != null)
            {
                foreach (var prefix in GetKeyPrefixes(name))
                {
                    LogLevel level;
                    if (settings.TryGetSwitch(prefix, out level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return falseFilter;
        }

        private IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }

        private IExternalScopeProvider GetScopeProvider()
        {
            if (_includeScopes && _scopeProvider == null)
            {
                _scopeProvider = new LoggerExternalScopeProvider();
            }
            return _includeScopes ? _scopeProvider : null;
        }

        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}
