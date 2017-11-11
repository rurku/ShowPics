using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Utilities.Cli;

namespace ShowPics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.HelpOption();
            ConfigureCommands(app);
            app.OnExecute(() => app.ShowHelp());
            app.Execute(args);
        }

        private static void ConfigureCommands(CommandLineApplication app)
        {
            var commands = new ICliCommand[]
            {
                new HostCommand()
            };

            foreach (var command in commands)
            {
                app.Command(command.CommandName, cla =>
                {
                    cla.ThrowOnUnexpectedArgument = false;
                    cla.Description = command.CommandDescription;
                    cla.HelpOption();
                    var builder = new CliOptionsBuilder(cla);
                    command.ConfigureOptions(builder);
                    cla.OnExecute(() => {
                        builder.ExecuteCallbacks();
                        command.Run(cla.RemainingArguments.ToArray());
                    });
                });
            }
        }
    }
}
