using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Utilities;
using ShowPics.Settings;
using Microsoft.Extensions.Configuration;
using ShowPics.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.IO;

namespace ShowPics
{
    public class CommonServiceConfiguration : ICommonServiceConfiguration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = (IConfiguration)services.Single(x => x.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            services.AddSingleton<ICommonServiceConfiguration>(this);
            services.AddOptions();
            var folderSettings = configuration.GetSection("folderSettings");
            services.Configure<FolderSettings>(folderSettings);
            
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.Cache = SqliteCacheMode.Default;
            connectionStringBuilder.DataSource = Path.Combine(folderSettings.Get<FolderSettings>().ThumbnailsPath, "data.db");
            connectionStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            services.AddDbContext<ShowPicsDbContext>(options =>
            {
                options.UseSqlite(connectionStringBuilder.ConnectionString);
            });
        }
    }
}
