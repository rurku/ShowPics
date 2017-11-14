using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShowPics.Cli.Jobs
{
    class RemoveFolderFromDisk : IJob
    {
        public RemoveFolderFromDisk(string physicalPath)
        {
            PhysicalPath = physicalPath;
        }

        public string PhysicalPath { get; }

        public string Description => $"Remove folder '{PhysicalPath}' from filesysem";

        public void Execute(IServiceProvider serviceProvider)
        {
            Directory.Delete(PhysicalPath, true);
        }
    }
}
