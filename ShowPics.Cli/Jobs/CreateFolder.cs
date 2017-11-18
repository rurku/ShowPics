using ShowPics.Data.Abstractions;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ShowPics.Utilities;
using ShowPics.Entities;
using Microsoft.Extensions.Logging;

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

        public void Execute(IServiceProvider serviceProvider)
        {
            var data = serviceProvider.GetService<IFilesData>();
            var pathHelper = serviceProvider.GetService<PathHelper>();
            var physicalPath = pathHelper.GetPhysicalPath(pathHelper.GetThumbnailPath(LogicalPath, false));
            serviceProvider.GetService<ILogger<CreateFolder>>().LogInformation("Creating folder {name}", physicalPath);
            Directory.CreateDirectory(physicalPath);
            var parentPath = pathHelper.GetParentPath(LogicalPath);
            var parent = data.GetFolder(parentPath);
            data.Add(new Folder()
            {
                Name = pathHelper.GetName(LogicalPath),
                ParentId = parent?.Id,
                Path = LogicalPath
            });
            data.SaveChanges();
        }
    }
}
