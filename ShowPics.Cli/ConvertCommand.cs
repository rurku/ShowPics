﻿using ShowPics.Utilities.Cli;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Cli.Jobs;
using Microsoft.Extensions.Logging;
using ShowPics.Cli.Executors;
using System.Linq;
using System.Threading.Tasks;

namespace ShowPics.Cli
{
    public class ConvertCommand : ICliCommand
    {
        public string CommandName => "convert";

        public string CommandDescription => "Create thumbnails and generate metadata";

        public void ConfigureOptions(ICliOptionsBuilder app)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<SynchronizedQueue<IJob>>();
            services.AddTransient<JobProducer>();
            services.AddTransient<JobExecutor<CreateFile>, DummyJobExecutor<CreateFile>>();
            services.AddTransient<JobExecutor<CreateFolder>, DummyJobExecutor<CreateFolder>>();
            services.AddTransient<JobExecutor<RemoveFileFromDb>, DummyJobExecutor<RemoveFileFromDb>>();
            services.AddTransient<JobExecutor<RemoveFileFromDisk>, DummyJobExecutor<RemoveFileFromDisk>>();
            services.AddTransient<JobExecutor<RemoveFolderFromDb>, DummyJobExecutor<RemoveFolderFromDb>>();
            services.AddTransient<JobExecutor<RemoveFolderFromDisk>, DummyJobExecutor<RemoveFolderFromDisk>>();
            services.AddTransient<JobExecutor<UpdateFile>, DummyJobExecutor<UpdateFile>>();
            services.AddTransient<JobConsumer>();
        }

        public void Run(string[] args, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<ConvertCommand>>();
            var jobQueue = serviceProvider.GetService<SynchronizedQueue<IJob>>();

            var producer = serviceProvider.GetService<JobProducer>();

            RunPipleline(4, producer, jobQueue, serviceProvider, (p, q) => p.RemoveNonExistingFromDb(q));
            RunPipleline(4, producer, jobQueue, serviceProvider, (p, q) => p.RemoveNonExistingFromDisk(q));
            RunPipleline(1, producer, jobQueue, serviceProvider, (p, q) => p.CreateFolders(q));
            RunPipleline(4, producer, jobQueue, serviceProvider, (p, q) => p.CreateOrUpdateThumbs(q));
        }

        void RunPipleline(int executorCount, JobProducer producer, SynchronizedQueue<IJob> queue, IServiceProvider serviceProvider, Action<JobProducer, SynchronizedQueue<IJob>> action)
        {
            var producerTask = Task.Run(() => action(producer, queue));
            var consumerTasks = Enumerable.Range(1, executorCount).Select(x => Task.Run(() => serviceProvider.GetService<JobConsumer>().Consume(queue))).ToArray();
            Task.WaitAll(producerTask);
            for (int i = 0; i < consumerTasks.Length; i++)
                queue.Enqueue(null);
            Task.WaitAll(consumerTasks.ToArray());
        }
    }
}