using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Rrs.Microsoft.Logging
{
    public interface IChannelLoggerSettings
    {
        bool IncludeScopes { get; }

        IChangeToken ChangeToken { get; }

        bool TryGetSwitch(string name, out LogLevel level);

        IChannelLoggerSettings Reload();
    }
}
