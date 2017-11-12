using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Jobs
{
    public class CreateFolder : IJob
    {
        public CreateFolder(string logicalPath)
        {
            LogicalPath = logicalPath;
        }
        public string Description => $"Create folder '{LogicalPath}'";

        public string LogicalPath { get; }
    }
}
