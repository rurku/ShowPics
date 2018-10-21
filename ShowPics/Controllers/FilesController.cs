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
        public ActionResult Get(string path, int depth = 1)
        {
            if (string.IsNullOrEmpty(path))
            {
                var folders = _filesData.GetTopLevelFolders(depth, Math.Max(depth - 1, 0));
                return Ok(new DirectoryDto()
                {
                    Name = "",
                    Path = _pathHelper.PathToUrl(_options.Value.OriginalsLogicalPrefix),
                    ApiPath = _pathHelper.PathToUrl(_pathHelper.GetApiPath(_options.Value.OriginalsLogicalPrefix)),
                    HasSubdirectories = folders.Any(),
                    Children = depth == 0 ? null : folders.OrderBy(x => x.Name).Select(x => MapToDto(x, depth - 1)).ToList()
                });
            }
            else
            {
                var folder = _filesData.GetFolder(_pathHelper.JoinLogicalPaths(_options.Value.OriginalsLogicalPrefix, path), depth + 1, depth);
                if (folder != null)
                    return Ok(MapToDto(folder, depth));
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
                ApiPath = _pathHelper.PathToUrl(_pathHelper.GetApiPath(file.Path)),
                Height = file.Height,
                Width = file.Width,
                ThumbnailPath = _pathHelper.PathToUrl(file.ThumbnailPath)
            };
        }

        FileSystemObject MapToDto(Entities.Folder folder, int? depth)
        {
            return new DirectoryDto()
            {
                Path = _pathHelper.PathToUrl(folder.Path),
                ApiPath = _pathHelper.PathToUrl(_pathHelper.GetApiPath(folder.Path)),
                Name = folder.Name,
                HasSubdirectories = folder.Children.Any(),
                Children = depth == 0
                    ? null
                    : folder.Children.OrderBy(x => x.Name)
                        .Select(x => MapToDto(x, depth - 1))
                        .Union(
                            folder.Files.OrderBy(x => x.OriginalCreationTime ?? x.ModificationTimestamp)
                            .Select(MapToDto)
                        )
                        .ToList()
            };
        }
    }
}
