using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli.Jobs
{
    class RemoveFolderFromDb : IJob
    {
        public RemoveFolderFromDb(Folder folder)
        {
            Folder = folder;
        }
        public string Description => $"Remove folder '{Folder.Path}' from DB";

        public Folder Folder { get; }
    }
}
