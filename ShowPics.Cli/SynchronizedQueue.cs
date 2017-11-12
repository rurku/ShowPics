using Microsoft.Extensions.Logging;
using ShowPics.Cli.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli
{
    class SynchronizedQueue<T> : ISynchronizedQueue<T>
    {
        private ILogger<SynchronizedQueue<T>> _logger;

        public SynchronizedQueue(ILogger<SynchronizedQueue<T>> logger)
        {
            _logger = logger;
        }

        public T Dequeue()
        {
            throw new NotImplementedException();
        }

        public void Enqueue(T item)
        {
            _logger.LogDebug($"Enqueue: {((IJob)item).Description}");
        }
    }
}
