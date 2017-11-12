using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Entities
{
    public class File
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public Folder Folder { get; set; }
    }
}
