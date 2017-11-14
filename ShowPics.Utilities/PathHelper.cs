using Microsoft.Extensions.Options;
using ShowPics.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ShowPics.Utilities
{
    public class PathHelper
    {
        private readonly IOptions<FolderSettings> _folderSettings;
        private Dictionary<string, string> _logicalToPhysicalRootMap = new Dictionary<string, string>();

        public PathHelper(IOptions<FolderSettings> folderSettings)
        {
            _folderSettings = folderSettings;

            _logicalToPhysicalRootMap.Add(_folderSettings.Value.ThumbnailsLogicalPrefix, _folderSettings.Value.ThumbnailsPath);
            foreach (var folder in _folderSettings.Value.Folders)
            {
                _logicalToPhysicalRootMap.Add(JoinLogicalPaths(_folderSettings.Value.OriginalsLogicalPrefix, folder.Name), folder.PhysicalPath);
            }
        }

        public string JoinLogicalPaths(params string[] paths)
        {
            var normalized = paths.Select(x => x.Trim('/')).Where(x => !string.IsNullOrEmpty(x));
            return string.Join('/', normalized);
        }

        public string GetPhysicalPath(string logicalPath)
        {
            foreach (var pair in _logicalToPhysicalRootMap)
            {
                if ((logicalPath + "/").StartsWith(pair.Key + "/"))
                {
                    return Path.Combine(pair.Value, logicalPath.Substring(pair.Key.Length).TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                }
            }
            throw new Exception($"Could not map logical path '{logicalPath}' to physical path.");
        }

        public string GetThumbnailPath(string originalPath)
        {
            if ((originalPath + "/").StartsWith(_folderSettings.Value.OriginalsLogicalPrefix + "/"))
            {
                return JoinLogicalPaths(_folderSettings.Value.ThumbnailsLogicalPrefix, originalPath.Substring(_folderSettings.Value.OriginalsLogicalPrefix.Length).TrimStart('/'));
            }
            throw new Exception($"Could not map original path '{originalPath}' to thumbnail path.");
        }

        public string GetParentPath(string logicalPath)
        {
            var split = logicalPath.Split('/', options: StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return null;
            return string.Join('/', split.Take(split.Length - 1));
        }

        public string GetName(string logicalPath)
        {
            var split = logicalPath.Split('/', options: StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return "";
            return split.Last();
        }
    }
}
