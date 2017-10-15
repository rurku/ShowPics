using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShowPics.Dtos;
using Microsoft.Extensions.Options;
using ShowPics.Settings;
using System.IO;

namespace ShowPics.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private static readonly Dictionary<string, string> _mimeTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {".jpg", "image/jpeg" }
        };
        private readonly IOptions<FolderSettings> _options;

        public FilesController(IOptions<FolderSettings> options)
        {
            _options = options;
        }

        [HttpGet("{*path}")]
        public ActionResult Get(string path, string format)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Ok(new DirectoryDto()
                {
                    Name = "",
                    Path = "/",
                    Children = _options.Value.Folders.Select(x => GetNodeDto("", x.PhysicalPath, x.Name)).Cast<FileSystemObject>().ToList()
                });
            }
            else
            {
                var pathSegments = path.Split('/');
                var rootDir = pathSegments.First();
                var physicalPath = _options.Value.Folders.FirstOrDefault(x => x.Name == rootDir)?.PhysicalPath;
                if (physicalPath == null)
                    throw new Exception($"root folder {rootDir} not found.");
                var remainingSegments = pathSegments.Skip(1);
                var node = GetNodeDto(JoinLogicalPaths(remainingSegments.ToArray()), physicalPath, rootDir);
                if (node is FileDto fileInfo && format != "json" && !string.IsNullOrEmpty(fileInfo.ContentType))
                {
                    var fileStream = System.IO.File.OpenRead(Path.Combine(remainingSegments.Prepend(physicalPath).ToArray()));
                    return File(fileStream, fileInfo.ContentType);
                }
                return Ok(node);
            }
        }

        static FileSystemObject GetNodeDto(string logicalPath, string rootPhysicalPath, string rootLogicalName)
        {
            var physicalPath = Path.Combine(rootPhysicalPath, logicalPath.Replace('/', Path.DirectorySeparatorChar));

            var fileAttr = System.IO.File.GetAttributes(physicalPath);
            if (fileAttr.HasFlag(FileAttributes.Directory))
            {
                var dirInfo = new DirectoryInfo(physicalPath);
                var result = new DirectoryDto()
                {
                    Name = string.IsNullOrEmpty(logicalPath) ? rootLogicalName : dirInfo.Name,
                    Path = JoinLogicalPaths(rootLogicalName, logicalPath)
                };
                foreach (var fullPath in (Directory.EnumerateFileSystemEntries(physicalPath)))
                {
                    var name = new FileInfo(fullPath).Name;
                    result.Children.Add(GetNodeDto(JoinLogicalPaths(logicalPath, name), rootPhysicalPath, rootLogicalName));
                }
                return result;
            }
            else
            {
                var fileInfo = new FileInfo(physicalPath);
                return new FileDto()
                {
                    Name = fileInfo.Name,
                    Path = JoinLogicalPaths(rootLogicalName, logicalPath),
                    ContentType = _mimeTypeMapping.GetValueOrDefault(fileInfo.Extension)
                };
            }
        }

        private static string JoinLogicalPaths(params string[] paths)
        {
            var normalized = paths.Select(x => x.Trim('/')).Where(x => !string.IsNullOrEmpty(x));
            return string.Join('/', normalized);
        }
    }
}
