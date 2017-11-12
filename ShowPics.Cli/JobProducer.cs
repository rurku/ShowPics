using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShowPics.Cli.Jobs;
using ShowPics.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using ShowPics.Entities;
using Microsoft.Extensions.DependencyInjection;
using ShowPics.Data.Abstractions;

namespace ShowPics.Cli
{
    public class JobProducer
    {
        private ILogger<JobProducer> _logger;
        private IOptions<FolderSettings> _folderSettings;
        private IServiceProvider _serviceProvider;
        private HashSet<string> _originFilePaths = new HashSet<string>();
        private HashSet<string> _originFolderPaths = new HashSet<string>();
        private HashSet<string> _thumbnailFilePaths = new HashSet<string>();
        private HashSet<string> _thumbnailFolderPaths = new HashSet<string>();
        private HashSet<string> _metadataFolderPaths = new HashSet<string>();
        private HashSet<string> _metadataFilePaths = new HashSet<string>();

        private Dictionary<string, string> _logicalToPhysicalRootMap = new Dictionary<string, string>();

        private string[] _filesToKeep =
            {
                "data.db"
            };


        public JobProducer(ILogger<JobProducer> logger, IOptions<FolderSettings> folderSettings, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _folderSettings = folderSettings;
            _serviceProvider = serviceProvider;

            _logicalToPhysicalRootMap.Add(_folderSettings.Value.ThumbnailsLogicalPrefix, _folderSettings.Value.ThumbnailsPath);
            foreach (var folder in _folderSettings.Value.Folders)
            {
                _logicalToPhysicalRootMap.Add(JoinLogicalPaths(_folderSettings.Value.OriginalsLogicalPrefix, folder.Name), folder.PhysicalPath);
            }
        }

        public void RemoveNonExistingFromDisk(ISynchronizedQueue<IJob> queue)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var data = scope.ServiceProvider.GetService<IFilesData>();
                foreach (var folder in data.GetAll())
                {
                    _metadataFolderPaths.Add(GetThumbnailPath(folder.Path));
                    foreach (var file in folder.Files)
                        _metadataFilePaths.Add(file.ThumbnailPath);
                }
            }

            ForEachFileSystemNode(_folderSettings.Value.ThumbnailsLogicalPrefix,
            (logicalPath, nodeType) =>
            {
                if (nodeType == NodeType.File && !_metadataFilePaths.Contains(logicalPath))
                {
                    var physicalPath = GetPhysicalPath(logicalPath);
                    if (_filesToKeep.Contains(Path.GetFileName(physicalPath)))
                        return false;

                    queue.Enqueue(new RemoveFileFromDisk(GetPhysicalPath(logicalPath)));
                    return false;
                }
                else if (nodeType == NodeType.Folder && !_metadataFolderPaths.Contains(logicalPath))
                {
                    queue.Enqueue(new RemoveFolderFromDisk(GetPhysicalPath(logicalPath)));
                    return false;
                }
                return true;
            }, false);

        }

        public void RemoveNonExistingFromDb(ISynchronizedQueue<IJob> queue)
        {
            ForEachFileSystemNode(_folderSettings.Value.OriginalsLogicalPrefix, 
                (logicalPath, nodeType) =>
                {
                    if (nodeType == NodeType.File)
                        _originFilePaths.Add(logicalPath);
                    else
                        _originFolderPaths.Add(logicalPath);
                    return true;
                }, false);

            using (var scope = _serviceProvider.CreateScope())
            {
                var data = scope.ServiceProvider.GetService<IFilesData>();
                RemoveNonExistingFromDbRecursively(data.GetAll().Where(x => x.ParentId == null).ToList(), queue);
            }
        }

        public void CreateOrUpdateThumbs(ISynchronizedQueue<IJob> queue)
        {
            var metadataByOriginalPath = new Dictionary<string, Entities.File>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var data = scope.ServiceProvider.GetService<IFilesData>();

                foreach (var folder in data.GetAll())
                {
                    foreach (var file in folder.Files)
                        metadataByOriginalPath.Add(file.Path, file);
                }
            }

            ForEachFileSystemNode(_folderSettings.Value.OriginalsLogicalPrefix,
                (logicalPath, nodeType) =>
                {
                    if (nodeType == NodeType.Folder)
                        return true;
                    if (!metadataByOriginalPath.ContainsKey(logicalPath))
                    {
                        queue.Enqueue(new CreateFile(logicalPath));
                        return false;
                    }
                    var physicalPath = GetPhysicalPath(logicalPath);
                    if (System.IO.File.GetLastWriteTime(physicalPath) != metadataByOriginalPath[logicalPath].ModificationTimestamp)
                        queue.Enqueue(new UpdateFile(logicalPath));

                    return false;
                }, false);
        }

        public void CreateFolders(ISynchronizedQueue<IJob> queue)
        {
            var metadataByOriginalPath = new Dictionary<string, Entities.File>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var data = scope.ServiceProvider.GetService<IFilesData>();

                foreach (var folder in data.GetAll())
                {
                    foreach (var file in folder.Files)
                        metadataByOriginalPath.Add(file.Path, file);
                }
            }

            ForEachFileSystemNode(_folderSettings.Value.OriginalsLogicalPrefix,
                (logicalPath, nodeType) =>
                {
                    if (nodeType == NodeType.File)
                        return false;
                    if (!metadataByOriginalPath.ContainsKey(logicalPath))
                        queue.Enqueue(new CreateFolder(logicalPath));
                    return true;
                }, false);
        }

        private void RemoveNonExistingFromDbRecursively(IEnumerable<Folder> folders, ISynchronizedQueue<IJob> queue)
        {
            foreach (var folder in folders)
            {
                if (!_originFolderPaths.Contains(folder.Path))
                {
                    queue.Enqueue(new RemoveFolderFromDb(folder));
                }
                else
                {
                    foreach (var file in folder.Files)
                    {
                        if (!_originFilePaths.Contains(file.Path))
                            queue.Enqueue(new RemoveFileFromDb(file));
                    }

                    RemoveNonExistingFromDbRecursively(folder.Children, queue);
                }
            }
        }

        private void ForEachFileSystemNode(string logicalPath, Func<string, NodeType, bool> action, bool includingRoot = true)
        {
            var children = GetChildren(logicalPath);
            if (children != null)
            {
                if (!includingRoot || action(logicalPath, NodeType.Folder))
                    foreach (var child in children)
                    {
                        ForEachFileSystemNode(JoinLogicalPaths(logicalPath, child), action);
                    }
            }
            else
            {
                action(logicalPath, NodeType.File);
            }
        }

        // returns null if path is not a directory
        private IEnumerable<string> GetChildren(string logicalPath)
        {
            var folderSettings = _folderSettings.Value;
            logicalPath = logicalPath.TrimEnd('/');
            if (logicalPath == "")
            {
                return new[] {
                    folderSettings.OriginalsLogicalPrefix.TrimEnd('/'),
                    folderSettings.ThumbnailsLogicalPrefix.TrimEnd('/')
                };
            }

            if (logicalPath == folderSettings.OriginalsLogicalPrefix.TrimEnd('/'))
            {
                return folderSettings.Folders.Select(x => x.Name).ToList();
            }

            var physicalPath = GetPhysicalPath(logicalPath);
            var fileAttr = System.IO.File.GetAttributes(physicalPath);
            if (fileAttr.HasFlag(FileAttributes.Directory))
            {
                return Directory.EnumerateFileSystemEntries(physicalPath).Select(x => Path.GetFileName(x));
            }
            else
                return null;
        }

        private string GetPhysicalPath(string logicalPath)
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

        private string GetThumbnailPath(string originalPath)
        {
            if ((originalPath + "/").StartsWith(_folderSettings.Value.OriginalsLogicalPrefix + "/"))
            {
                return JoinLogicalPaths(_folderSettings.Value.ThumbnailsLogicalPrefix, originalPath.Substring(_folderSettings.Value.OriginalsLogicalPrefix.Length).TrimStart('/'));
            }
            throw new Exception($"Could not map original path '{originalPath}' to thumbnail path.");
        }

        private enum NodeType
        {
            File,
            Folder
        }

        private static string JoinLogicalPaths(params string[] paths)
        {
            var normalized = paths.Select(x => x.Trim('/')).Where(x => !string.IsNullOrEmpty(x));
            return string.Join('/', normalized);
        }
    }


}
