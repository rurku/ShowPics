using Microsoft.Extensions.Logging;
using ShowPics.Cli.Jobs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ShowPics.Cli
{
    class SynchronizedQueue<T> : ISynchronizedQueue<T>
    {
        private const int QUEUE_SIZE = 20;
        private ILogger<SynchronizedQueue<T>> _logger;
        private Semaphore _readSemaphore = new Semaphore(0, QUEUE_SIZE);
        private Semaphore _writeSemaphore = new Semaphore(QUEUE_SIZE, QUEUE_SIZE);
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public SynchronizedQueue(ILogger<SynchronizedQueue<T>> logger)
        {
            _logger = logger;
        }

        public T Dequeue()
        {
            while (System.IO.File.Exists("pause"))
                Thread.Sleep(1000);
            T result;
            _readSemaphore.WaitOne();
            _queue.TryDequeue(out result); // ignore return value because semaphores guarantee that there will always be something in the queue
            _writeSemaphore.Release();
            return result;
        }

        public void Enqueue(T item)
        {
            _writeSemaphore.WaitOne();
            _queue.Enqueue(item);
            _readSemaphore.Release();
        }
    }
}
