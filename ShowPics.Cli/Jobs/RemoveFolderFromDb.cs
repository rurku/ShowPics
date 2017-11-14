using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Data.Abstractions;

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

        public void Execute(IServiceProvider serviceProvider)
        {
            var data = serviceProvider.GetService<IFilesData>();
            data.Remove(Folder);
            data.SaveChanges();
        }
    }
}
