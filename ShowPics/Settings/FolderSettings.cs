using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowPics.Settings
{
    public class FolderSettings
    {
        public List<RootFolderMapping> Folders { get; set; }
        public string ThumbnailsPath { get; set; }
    }

    public class RootFolderMapping
    {
        public string Name { get; set; }
        public string PhysicalPath { get; set; }
    }
}
