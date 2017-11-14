﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShowPics.Cli.Executors;
using ShowPics.Cli.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

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
                    var serviceType = typeof(JobExecutor<>).MakeGenericType(job.GetType());
                    var executor = serviceScope.ServiceProvider.GetService(serviceType);
                    var executeMethod = serviceType.GetMethod("Execute");
                    executeMethod.Invoke(executor, new object[] { job });
                }
            }
        }
    }
}
