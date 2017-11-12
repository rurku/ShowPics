using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Jobs
{
    public class CreateFile : IJob
    {
        public CreateFile(string logicalPath)
        {
            LogicalPath = logicalPath;
        }
        public string Description => $"Create file '{LogicalPath}'";

        public string LogicalPath { get; }
    }
}
