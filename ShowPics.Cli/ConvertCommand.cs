using ShowPics.Utilities.Cli;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Cli.Jobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using ShowPics.Utilities;
using Microsoft.Extensions.Options;
using ShowPics.Utilities.Settings;

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
            //services.AddTransient<SynchronizedQueue<IJob>>();
            //services.AddTransient<JobProducer>();
            //services.AddTransient<JobConsumer>();
            services.AddTransient<ConversionRunner>();
        }

        public void Run(string[] args, IServiceProvider serviceProvider)
        {
            serviceProvider.GetService<ConversionRunner>().Run();
        }

        //void RunPipleline(int executorCount, JobProducer producer, SynchronizedQueue<IJob> queue, IServiceProvider serviceProvider, Action<JobProducer, SynchronizedQueue<IJob>> action)
        //{
        //    var producerTask = Task.Run(() => action(producer, queue));
        //    var consumerTasks = Enumerable.Range(1, executorCount).Select(x => Task.Run(() => serviceProvider.GetService<JobConsumer>().Consume(queue))).ToArray();
        //    Task.WaitAll(producerTask);
        //    for (int i = 0; i < consumerTasks.Length; i++)
        //        queue.Enqueue(null);
        //    Task.WaitAll(consumerTasks.ToArray());
        //}
    }
}
