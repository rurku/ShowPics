using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Jobs
{
    class RemoveFileFromDb : IJob
    {
        public RemoveFileFromDb(File file)
        {
            File = file;
        }
        public string Description => $"Remove file '{File.Path}' from DB";

        public File File { get; }
    }
}
