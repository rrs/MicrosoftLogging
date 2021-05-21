using System.Threading.Channels;

namespace Rrs.Microsoft.Logging
{
    public class LoggerChannel
    {
        public Channel<string> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<string>();
    }
}
