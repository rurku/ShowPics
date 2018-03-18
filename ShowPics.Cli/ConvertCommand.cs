using ShowPics.Utilities.Cli;
using System;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddTransient<ConversionRunner>();
            services.AddTransient<Func<string, IThumbnailCreator>>(sp => s => ActivatorUtilities.CreateInstance<ThumbnailCreator>(sp, s));
        }

        public void Run(string[] args, IServiceProvider serviceProvider)
        {
            serviceProvider.GetService<ConversionRunner>().Run();
        }
    }
}
