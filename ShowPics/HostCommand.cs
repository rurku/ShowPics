using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Cli;
using Microsoft.Extensions.DependencyInjection;
using Utilities;

namespace ShowPics
{
    public class HostCommand : ICliCommand
    {
        public string CommandName => "host";
        public string CommandDescription => "Start the web application";

        public void ConfigureOptions(ICliOptionsBuilder app)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Run(string[] args, IServiceProvider serviceProvider)
        {
            BuildWebHost(args, serviceProvider).Run();
        }

        public IWebHost BuildWebHost(string[] args, IServiceProvider serviceProvider)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .ConfigureServices(serviceProvider.GetService<ICommonServiceConfiguration>().ConfigureServices)
                .UseStartup<Startup>()
                .Build();
        }

    }
}
