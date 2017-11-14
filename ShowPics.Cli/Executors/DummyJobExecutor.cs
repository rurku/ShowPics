using Microsoft.Extensions.Logging;
using ShowPics.Cli.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Executors
{
    public class DummyJobExecutor<T> : JobExecutor<T>
        where T : IJob
    {
        private readonly ILogger<DummyJobExecutor<T>> _logger;

        public DummyJobExecutor(ILogger<DummyJobExecutor<T>> logger)
        {
            _logger = logger;
        }

        public override void Execute(T job)
        {
            _logger.LogInformation($"Executing {job.Description}");
        }
    }
}
