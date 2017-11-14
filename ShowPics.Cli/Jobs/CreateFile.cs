using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Utilities;
using SixLabors.ImageSharp;
using System.IO;
using ShowPics.Data.Abstractions;

namespace ShowPics.Cli.Jobs
{
    public class CreateOrUpdateFile : IJob
    {
        public CreateOrUpdateFile(string logicalPath)
        {
            LogicalPath = logicalPath;
        }
        public string Description => $"Create file '{LogicalPath}'";

        public string LogicalPath { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            var pathHelper = serviceProvider.GetService<PathHelper>();
            var originalPhysicalPath = pathHelper.GetPhysicalPath(LogicalPath);
            var thumbnailPath = pathHelper.GetThumbnailPath(LogicalPath);
            var thumbnailPhysicalPath = pathHelper.GetPhysicalPath(thumbnailPath);

            var data = serviceProvider.GetService<IFilesData>();
            var file = data.GetFile(LogicalPath) ?? new Entities.File();

            file.ModificationTimestamp = File.GetLastWriteTime(originalPhysicalPath);
            file.Name = pathHelper.GetName(LogicalPath);
            file.Path = LogicalPath;
            file.ThumbnailPath = thumbnailPath;

            using (var image = Image.Load(originalPhysicalPath))
            {
                file.Width = image.Width;
                file.Height = image.Height;
                var desiredHeight = 200;
                var desiredWidth = image.Width * desiredHeight / image.Height;
                image.Mutate(x => x.Resize(desiredWidth, desiredHeight));
                image.Save(thumbnailPhysicalPath);
            }

            if (file.Id == 0)
            {
                file.FolderId = data.GetFolder(pathHelper.GetParentPath(LogicalPath)).Id;
                data.Add(file);
            }
            data.SaveChanges();
        }
    }
}
