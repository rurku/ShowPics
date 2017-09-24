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
        private readonly IOptions<FolderSettings> _options;

        public FilesController(IOptions<FolderSettings> options)
        {
            _options = options;
        }

        [HttpGet("{*path}")]
        public FileSystemObject Get(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new DirectoryDto()
                {
                    Name = "",
                    Path = "/",
                    Children = _options.Value.Folders.Select(x => GetRootDirectory(x.Name, x.PhysicalPath)).Cast<FileSystemObject>().ToList()
                };
            }
            else
            {
                var pathSegments = path.Split('/');
                var rootDir = pathSegments.First();
                var physicalPath = _options.Value.Folders.FirstOrDefault(x => x.Name == rootDir)?.PhysicalPath;
                if (physicalPath == null)
                    throw new Exception($"root folder {rootDir} not found.");
                var remainingSegments = pathSegments.Skip(1);
                if (!remainingSegments.Any())
                {
                    return GetRootDirectory(rootDir, physicalPath);
                }
                else
                {
                    return DirSearch(Path.Combine(remainingSegments.ToArray()), physicalPath, rootDir);
                }

            }
        }

        private DirectoryDto GetRootDirectory(string name, string physicalPath)
        {
            var result = DirSearch("", physicalPath, name);
            result.Name = name;
            return result;
        }

        static DirectoryDto DirSearch(string path, string rootPath, string rootName)
        {
            var dirPath = Path.Combine(rootPath, path.Replace('/', Path.DirectorySeparatorChar));
            var info = new DirectoryInfo(dirPath);
            var result = new DirectoryDto()
            {
                Name = info.Name,
                Path = JoinPaths(rootName, path)
            };

            foreach (string d in Directory.GetDirectories(dirPath))
            {
                var dirInfo = new DirectoryInfo(d);
                result.Children.Add(DirSearch(JoinPaths(path, dirInfo.Name), rootPath, rootName));
            }

            foreach (string f in Directory.GetFiles(dirPath))
            {
                var fileInfo = new FileInfo(f);
                result.Children.Add(new FileDto()
                {
                    Name = fileInfo.Name,
                    Path = JoinPaths(path, fileInfo.Name)
                });
            }
            return result;
        }

        private static string JoinPaths(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return b;
            }
            else if (string.IsNullOrEmpty(b))
            {
                return a;
            }
            else
                return $"{a}/{b}";
        }
    }
}
