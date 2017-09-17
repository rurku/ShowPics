using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Dtos
{
    public class DirectoryDto : FileSystemObject
    {
        public IList<FileSystemObject> Children { get; set; } = new List<FileSystemObject>();
    }
}
