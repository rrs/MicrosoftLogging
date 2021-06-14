using Microsoft.Extensions.Logging;

namespace Rrs.Microsoft.Logging
{
    public class ChannelLoggerOptions
    {
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// Gets or sets format string used to format timestamp in logging messages. Defaults to <c>null</c>
        /// </summary>
        public string TimestampFormat { get; set; }
    }
}
