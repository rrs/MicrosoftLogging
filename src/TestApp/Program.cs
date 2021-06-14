using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Rrs.Microsoft.Logging;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            var channel = host.Services.GetRequiredService<LoggerChannel>().Channel;
            Task.Run(async () => 
            {
                while (await channel.Reader.WaitToReadAsync())
                    Console.WriteLine(render(await channel.Reader.ReadAsync()));
            });
            await host.RunAsync();
        }

        static string render(Log log) => string.Concat(log.Scope.Select(o => $"{o} => ")) + log.Message;

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
                .ConfigureLogging(logging =>
                {
                    logging.AddChannelLogger();
                    logging.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<TestAppHostedService>();
                });
    }
}
