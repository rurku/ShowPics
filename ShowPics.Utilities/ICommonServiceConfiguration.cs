using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Utilities
{
    public interface ICommonServiceConfiguration
    {
        void ConfigureServices(IServiceCollection services);
    }
}
