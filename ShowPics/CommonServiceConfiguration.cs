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

namespace ShowPics
{
    public class CommonServiceConfiguration : ICommonServiceConfiguration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = (IConfiguration)services.Single(x => x.ServiceType == typeof(IConfiguration)).ImplementationInstance;
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
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
        }
    }
}
