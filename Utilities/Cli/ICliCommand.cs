using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Cli
{
    public interface ICliCommand
    {
        string CommandName { get; }
        string CommandDescription { get; }
        void ConfigureOptions(ICliOptionsBuilder app);
        void ConfigureServices(IServiceCollection services);
        void Run(string[] args, IServiceProvider serviceProvider);
    }
}
