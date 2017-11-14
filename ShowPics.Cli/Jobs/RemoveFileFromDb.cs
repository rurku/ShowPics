using ShowPics.Data.Abstractions;
using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

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

        public void Execute(IServiceProvider serviceProvider)
        {
            var data = serviceProvider.GetService<IFilesData>();
            data.Remove(File);
            data.SaveChanges();
        }
    }
}
