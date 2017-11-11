using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Cli;

namespace ShowPics
{
    public class HostCommand : ICliCommand
    {
        public string CommandName => "host";
        public string CommandDescription => "Start the web application";

        public void ConfigureOptions(ICliOptionsBuilder app)
        {
        }

        public void Run(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
