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
using Microsoft.Extensions.DependencyInjection;
using Utilities;
using ShowPics.Data;
using Microsoft.EntityFrameworkCore;

namespace ShowPics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("RUNS_IN_IIS_EXPRESS") == "true" && args.Length == 0)
                args = new string[] { "host" };

            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.HelpOption();
            ConfigureCommands(app);
            app.OnExecute(() => app.ShowHelp());
            app.Execute(args);
        }

        public static IServiceCollection BuildServiceCollection()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IConfiguration>(config);

            var commonConfiguration = new CommonServiceConfiguration();
            serviceCollection.AddSingleton<ICommonServiceConfiguration>(commonConfiguration);
            commonConfiguration.ConfigureServices(serviceCollection);

            return serviceCollection;
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
                        var services = BuildServiceCollection();
                        command.ConfigureServices(services);
                        using (var serviceProvider = services.BuildServiceProvider())
                        {
                            MigrateDatabase(serviceProvider);
                            command.Run(cla.RemainingArguments.ToArray(), serviceProvider);
                        }
                    });
                });
            }
        }

        private static void MigrateDatabase(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                serviceScope.ServiceProvider.GetService<ShowPicsDbContext>().Database.Migrate();
            }
        }
    }
}
