using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Channels;

namespace Rrs.Microsoft.Logging
{
    [ProviderAlias("File")]
    public class ChannelLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IDisposable _onChangeToken;
        private Channel<string> _channel;
        private ChannelLoggerOptions _options;
        private IExternalScopeProvider _scopeProvider;

        private IExternalScopeProvider ScopeProvider => _scopeProvider ??= new LoggerExternalScopeProvider();

        public ChannelLoggerProvider(IOptionsMonitor<ChannelLoggerOptions> options, LoggerChannel channel) : this(options.CurrentValue, channel)
        {
            _onChangeToken = options.OnChange(options => {
                _options = options;
            });
        }

        public ChannelLoggerProvider(ChannelLoggerOptions options, LoggerChannel channel) => (_options, _channel) = (options, channel.Channel);

        public ILogger CreateLogger(string name) => new ChannelLogger(name, _options, ScopeProvider, _channel);

        public void Dispose() => _onChangeToken?.Dispose();

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;
    }
}
