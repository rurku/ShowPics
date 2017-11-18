using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowPics.Utilities.Settings
{
    public class FolderSettings
    {
        public List<RootFolderMapping> Folders { get; set; }
        public string ThumbnailsPath { get; set; }
        public string ThumbnailsLogicalPrefix { get; set; } = "thumbnails";
        public string OriginalsLogicalPrefix { get; set; } = "files";
        public int ConvertionThreads { get; set; } = 1;
    }

    public class RootFolderMapping
    {
        public string Name { get; set; }
        public string PhysicalPath { get; set; }
    }
}
