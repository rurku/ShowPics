using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Dtos
{
    public class FileDto : FileSystemObject
    {
        [JsonProperty(Order = 0)]
        public string ContentType;
    }
}
