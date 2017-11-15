using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShowPics.Dtos;
using Microsoft.Extensions.Options;
using ShowPics.Utilities.Settings;
using System.IO;
using ShowPics.Data.Abstractions;
using ShowPics.Utilities;

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
        private readonly IFilesData _filesData;
        private readonly PathHelper _pathHelper;

        public FilesController(IOptions<FolderSettings> options, IFilesData filesData, PathHelper pathHelper)
        {
            _options = options;
            _filesData = filesData;
            _pathHelper = pathHelper;
        }

        [HttpGet("{*path}")]
        public ActionResult Get(string path)
        {
            var folders = _filesData.GetAll();

            if (string.IsNullOrEmpty(path))
            {
                return Ok(new DirectoryDto()
                {
                    Name = "",
                    Path = "/",
                    Children = folders.Where(x => x.ParentId == null).Select(x => MapToDto(x)).ToList()
                });
            }
            else
            {
                var folder = folders.SingleOrDefault(x => x.Path == _pathHelper.JoinLogicalPaths(_options.Value.OriginalsLogicalPrefix, path));
                if (folder != null)
                    return Ok(MapToDto(folder));
                var file = _filesData.GetFile(_pathHelper.JoinLogicalPaths(_options.Value.OriginalsLogicalPrefix, path));
                if (file != null)
                    return Ok(MapToDto(file));
                return NotFound();
            }
        }

        FileSystemObject MapToDto(Entities.File file)
        {
            return new FileDto()
            {

                ContentType = _mimeTypeMapping.GetValueOrDefault(Path.GetExtension(file.Name)),
                Name = file.Name,
                Path = _pathHelper.PathToUrl(file.Path),
                Height = file.Height,
                Width = file.Width,
                ThumbnailPath = _pathHelper.PathToUrl(file.ThumbnailPath)
            };
        }

        FileSystemObject MapToDto(Entities.Folder folder)
        {
            return new DirectoryDto()
            {
                Path = _pathHelper.PathToUrl(folder.Path),
                Name = folder.Name,
                Children = folder.Children.Select(MapToDto).Union(folder.Files.Select(MapToDto)).ToList()
            };
        }
    }
}
