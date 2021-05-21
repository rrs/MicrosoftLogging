using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class TestAppHostedService : IHostedService
    {
        private readonly ILogger<TestAppHostedService> _logger;

        public TestAppHostedService(ILogger<TestAppHostedService> logger) => _logger = logger;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Start");

            using var outer = _logger.BeginScope("Outer Scope");
            _logger.LogInformation("Hello");
            using var inner = _logger.BeginScope("Inner Scope");
            _logger.LogInformation("World");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
