using System.Threading.Channels;

namespace Rrs.Microsoft.Logging
{
    public class LoggerChannel
    {
        public Channel<Log> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<Log>();
    }
}
