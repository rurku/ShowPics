using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
