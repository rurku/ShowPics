using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public interface ICommonServiceConfiguration
    {
        void ConfigureServices(IServiceCollection services);
    }
}
