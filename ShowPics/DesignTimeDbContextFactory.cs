using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowPics
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShowPicsDbContext>
    {
        public ShowPicsDbContext CreateDbContext(string[] args)
        {
            var serviceCollection = Program.BuildServiceCollection();
            var provider = serviceCollection.BuildServiceProvider();
            return provider.GetService<ShowPicsDbContext>();
        }
    }
}
