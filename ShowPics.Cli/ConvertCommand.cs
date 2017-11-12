using ShowPics.Utilities.Cli;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Cli.Jobs;
using Microsoft.Extensions.Logging;

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
        }

        public void Run(string[] args, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<ConvertCommand>>();
            var jobQueue = serviceProvider.GetService<SynchronizedQueue<IJob>>();
            var producer = serviceProvider.GetService<JobProducer>();
            producer.RemoveNonExistingFromDb(jobQueue);
            producer.RemoveNonExistingFromDisk(jobQueue);
            producer.CreateFolders(jobQueue);
            producer.CreateOrUpdateThumbs(jobQueue);
        }
    }
}
