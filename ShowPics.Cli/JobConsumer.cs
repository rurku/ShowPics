using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShowPics.Cli.Jobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ShowPics.Cli
{
    public class JobConsumer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobConsumer> _logger;

        public JobConsumer(IServiceProvider serviceProvider, ILogger<JobConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Consume(ISynchronizedQueue<IJob> queue)
        {
            IJob job;
            while ((job = queue.Dequeue()) != null)
            {
                using (var serviceScope = _serviceProvider.CreateScope())
                {
                    _logger.LogInformation($"Executing: {job.Description}");
                    try
                    {
                        job.Execute(serviceScope.ServiceProvider);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed:    {job.Description}");
                    }
                    _logger.LogInformation($"Done:      {job.Description}");
                }
            }
            _logger.LogInformation($"Consumer thread {Thread.CurrentThread.ManagedThreadId} finished.");
        }
    }
}
