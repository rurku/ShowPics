using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Dtos
{
    public abstract class FileSystemObject
    {
        public string Type => GetType().Name;
        public virtual string Path { get; set; }
        public virtual string Name { get; set; }
    }
}
