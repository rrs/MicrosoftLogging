using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rrs.Microsoft.Logging
{
    public class ChannelLogger : ILogger
    {
        private readonly string _name;
        private readonly ChannelLoggerOptions _options;
        private readonly IExternalScopeProvider _scopeProvider;
        private readonly string _padding = " ";
        private readonly Channel<string> _channel;

        public ChannelLogger(string name, ChannelLoggerOptions options, IExternalScopeProvider scopeProvider, Channel<string> channel)
            => (_name, _options, _scopeProvider, _channel) = (name, options, scopeProvider, channel);

        public IDisposable BeginScope<TState>(TState state) => _scopeProvider?.Push(state) ?? default;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None
            && _options.LogLevel != LogLevel.None
            && Convert.ToInt32(logLevel) >= Convert.ToInt32(_options.LogLevel);


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message) && exception == null) return;

            var logBuilder = new StringBuilder();


            logBuilder.Append(_name);
            logBuilder.Append("[");
            logBuilder.Append(eventId);
            logBuilder.Append("]");

            AddScopeInformation(logBuilder);

            if (!string.IsNullOrEmpty(message))
            {
                logBuilder.Append(_padding);
                logBuilder.Append(message);
            }


            _channel.Writer.WriteAsync(logBuilder.ToString());
        }

        private void AddScopeInformation(StringBuilder stringBuilder)
        {
            var scopeProvider = _scopeProvider;
            if (scopeProvider != null)
            {
                var initialLength = stringBuilder.Length;

                scopeProvider.ForEachScope((scope, state) =>
                {
                    var (builder, length) = state;
                    var first = length == builder.Length;
                    builder.Append(first ? "=> " : " => ").Append(scope);
                }, (stringBuilder, initialLength));

                if (stringBuilder.Length > initialLength)
                {
                    stringBuilder.Append(" => ");
                }
            }
        }
    }
}
