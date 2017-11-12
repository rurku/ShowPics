using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Jobs
{
    public class UpdateFile : IJob
    {
        public UpdateFile(string logicalPath)
        {
            LogicalPath = logicalPath;
        }
        public string Description => $"Update file '{LogicalPath}'";

        public string LogicalPath { get; }
    }
}
