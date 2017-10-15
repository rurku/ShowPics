using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Dtos
{
    public class DirectoryDto : FileSystemObject
    {
        [JsonProperty(Order = 0)]
        public IList<FileSystemObject> Children { get; set; } = new List<FileSystemObject>();
    }
}
