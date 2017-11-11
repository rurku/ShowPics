using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Utilities;
using ShowPics.Settings;
using Microsoft.Extensions.Configuration;

namespace ShowPics
{
    public class CommonServiceConfiguration : ICommonServiceConfiguration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = (IConfiguration)services.Single(x => x.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            services.AddSingleton<ICommonServiceConfiguration>(this);
            services.AddOptions();
            services.Configure<FolderSettings>(configuration.GetSection("folderSettings"));
        }
    }
}
