using McMaster.Extensions.CommandLineUtils;
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
        void Run(string[] args);
    }
}
