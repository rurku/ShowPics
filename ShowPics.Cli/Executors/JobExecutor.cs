using ShowPics.Cli.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Executors
{
    public abstract class JobExecutor<T>
        where T : IJob
    {
        public abstract void Execute(T job);
    }
}
