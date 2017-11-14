using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Entities
{
    public class Folder
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public List<File> Files { get; set; } = new List<File>();
        public List<Folder> Children { get; set; } = new List<Folder>();
        public long? ParentId { get; set; }
        public Folder Parent { get; set; }
    }
}
