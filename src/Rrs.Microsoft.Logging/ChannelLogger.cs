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
        /*private readonly string _name;
        private readonly ChannelLoggerOptions _options;
        private readonly IExternalScopeProvider _scopeProvider;
        private readonly string _padding = " ";
        private readonly Channel<string> _channel;

        public ChannelLogger(string name, ChannelLoggerOptions options, IExternalScopeProvider scopeProvider, Channel<string> channel)
            => (_name, _options, _scopeProvider, _channel) = (name, options, scopeProvider, channel);

        public IDisposable BeginScope<TState>(TState state) => _scopeProvider?.Push(state) ?? default;

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                return false;
            }

            return Filter(Name, logLevel);
        }

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
                    builder.Append(Environment.NewLine + "\t=> ").Append(scope);
                }, (stringBuilder, initialLength));

                if (stringBuilder.Length > initialLength)
                {
                    stringBuilder.Append(Environment.NewLine + "\t=>");
                }
            }
        }*/

        private readonly Channel<Log> _channel;
        private Func<string, LogLevel, bool> _filter;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        //public ChannelLogger(string name, Func<string, LogLevel, bool> filter, bool includeScopes)
        //    : this(name, filter, includeScopes ? new LoggerExternalScopeProvider() : null)
        //{
        //}

        public ChannelLogger(string name, Func<string, LogLevel, bool> filter, IExternalScopeProvider scopeProvider, Channel<Log> channel)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _channel = channel;
            Name = name;
            Filter = filter ?? ((category, logLevel) => true);
            ScopeProvider = scopeProvider;
        }

        public Func<string, LogLevel, bool> Filter
        {
            get { return _filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _filter = value;
            }
        }

        public string Name { get; }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        internal string TimestampFormat { get; set; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);


            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                var log = new Log
                {
                    LogName = Name,
                    EventId = eventId,
                    Message = message,
                    Exception = exception,
                    LogLevel = logLevel,
                    Scope = GetScopeInformation()
                };

                _channel.Writer.WriteAsync(log);
            }
        }

        private List<object> GetScopeInformation()
        {
            var scopes = new List<object>();
            var scopeProvider = ScopeProvider;
            if (scopeProvider != null)
            {
                scopeProvider.ForEachScope((scope, scopes) =>
                {
                    scopes.Add(scope);
                }, scopes);
            }

            return scopes;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                return false;
            }

            return Filter(Name, logLevel);
        }

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;
    }
}
