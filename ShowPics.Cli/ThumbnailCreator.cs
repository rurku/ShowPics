using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Utilities;
using SixLabors.ImageSharp;
using System.IO;
using ShowPics.Data.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.MetaData;
using System.Globalization;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;

namespace ShowPics.Cli
{
    public interface IThumbnailCreator
    {
        bool IsFormatSupported();
        void CreateOrUpdateThumbnail();
    }


    public class ThumbnailCreator : IThumbnailCreator
    {
        public ThumbnailCreator(string logicalPath, IFilesData filesData, PathHelper pathHelper)
        {
            LogicalPath = logicalPath;
            _filesData = filesData;
            _pathHelper = pathHelper;
        }

        private static readonly string[] _imageFileTypes =
        {
            ".jpg$",
            ".png$",
            ".gif$",
            ".bmp$"
        };

        private readonly IFilesData _filesData;
        private readonly PathHelper _pathHelper;

        public bool IsFormatSupported() 
            => _imageFileTypes.Any(x => Regex.IsMatch(LogicalPath, x, RegexOptions.IgnoreCase));

        public string LogicalPath { get; }

        public void CreateOrUpdateThumbnail()
        {
            var originalPhysicalPath = _pathHelper.GetPhysicalPath(LogicalPath);
            var thumbnailPath = _pathHelper.GetThumbnailPath(LogicalPath, true);
            var thumbnailPhysicalPath = _pathHelper.GetPhysicalPath(thumbnailPath);

            var file = _filesData.GetFile(LogicalPath) ?? new Entities.File();
            var fileTimestamp = File.GetLastWriteTime(originalPhysicalPath);
            file.ModificationTimestamp = new DateTime(fileTimestamp.Year, fileTimestamp.Month, fileTimestamp.Day, fileTimestamp.Hour, fileTimestamp.Minute, fileTimestamp.Second);
            file.Name = _pathHelper.GetName(LogicalPath);
            file.Path = LogicalPath;
            file.ThumbnailPath = thumbnailPath;

            using (var image = Image.Load(originalPhysicalPath))
            {
                file.Width = image.Width;
                file.Height = image.Height;
                file.OriginalCreationTime = GetOriginalCreationDate(image.MetaData);
                var desiredHeight = 200;
                var desiredWidth = image.Width * desiredHeight / image.Height;
                image.Mutate(x => x.Resize(desiredWidth, desiredHeight));
                image.Save(thumbnailPhysicalPath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
            }

            if (file.Id == 0)
            {
                file.FolderId = _filesData.GetFolder(_pathHelper.GetParentPath(LogicalPath)).Id;
                _filesData.Add(file);
            }
            _filesData.SaveChanges();
        }

        private DateTime? GetOriginalCreationDate(ImageMetaData metaData)
        {
            var dateString = metaData?.ExifProfile?.Values?.SingleOrDefault(x => x.Tag == SixLabors.ImageSharp.MetaData.Profiles.Exif.ExifTag.DateTimeOriginal)?.Value as string;
            if (string.IsNullOrEmpty(dateString))
                return null;
            DateTime dateTime;
            if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                return dateTime;
            return null;
        }
    }
}
