using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Jobs
{
    public interface IJob
    {
        string Description { get; }

        void Execute(IServiceProvider serviceProvider);
    }
}
