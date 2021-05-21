using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Rrs.Microsoft.Logging
{
    internal class ChannelLoggerOptionsSetup : ConfigureFromConfigurationOptions<ChannelLoggerOptions>
    {
        /// <summary>
        /// Constructor that takes the IConfiguration instance to bind against.
        /// </summary>
        public ChannelLoggerOptionsSetup(ILoggerProviderConfiguration<ChannelLoggerProvider> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}
