using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Utilities;
using ShowPics.Utilities.Settings;
using Microsoft.Extensions.Configuration;
using ShowPics.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.IO;
using ShowPics.Data.Abstractions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;
using Serilog.Events;

namespace ShowPics
{
    /// <summary>
    /// Service configuration that's used in web server context as well as in command line interface
    /// </summary>
    public class CommonServiceConfiguration : ICommonServiceConfiguration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = (IConfiguration)services.Single(x => x.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(logger, dispose: true);
            });
            // Configuration options
            services.AddOptions();
            var folderSettings = configuration.GetSection("folderSettings");
            services.Configure<FolderSettings>(folderSettings);

            // DbContext
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.Cache = SqliteCacheMode.Default;
            connectionStringBuilder.DataSource = Path.Combine(folderSettings.Get<FolderSettings>().ThumbnailsPath, "data.db");
            connectionStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            services.AddDbContext<ShowPicsDbContext>(options =>
            {
                options.UseSqlite(connectionStringBuilder.ConnectionString);
            });

            // DAL
            services.AddScoped<IFilesData, FilesData>();

            services.AddSingleton<PathHelper>();

        }
    }
}
