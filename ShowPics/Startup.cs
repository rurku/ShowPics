using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShowPics.Utilities.Settings;
using Microsoft.Extensions.FileProviders;

namespace ShowPics
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<FolderSettings> folderSettings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = "/" + folderSettings.Value.ThumbnailsLogicalPrefix,
                FileProvider = new PhysicalFileProvider(folderSettings.Value.ThumbnailsPath)
            });

            foreach (var folderSetting in folderSettings.Value.Folders)
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    RequestPath = "/" + folderSettings.Value.OriginalsLogicalPrefix + "/" + folderSetting.Name,
                    FileProvider = new PhysicalFileProvider(folderSetting.PhysicalPath)
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
