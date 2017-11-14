using ShowPics.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace ShowPics.Cli.Jobs
{
    class RemoveFileFromDisk : IJob
    {
        public RemoveFileFromDisk(string physicalPath)
        {
            PhysicalPath = physicalPath;
        }

        public string PhysicalPath { get; }

        public string Description => $"Remove file '{PhysicalPath}' from filesysem";

        public void Execute(IServiceProvider serviceProvider)
        {
            var pathHelper = serviceProvider.GetService<PathHelper>();
            File.Delete(PhysicalPath);
        }
    }
}
