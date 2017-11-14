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
using ShowPics.Utilities;

namespace ShowPics.Cli
{
    public class JobProducer
    {
        private ILogger<JobProducer> _logger;
        private IOptions<FolderSettings> _folderSettings;
        private readonly PathHelper _pathHelper;
        private IServiceProvider _serviceProvider;
        private HashSet<string> _originFilePaths = new HashSet<string>();
        private HashSet<string> _originFolderPaths = new HashSet<string>();
        private HashSet<string> _thumbnailFilePaths = new HashSet<string>();
        private HashSet<string> _thumbnailFolderPaths = new HashSet<string>();
        private HashSet<string> _metadataFolderPaths = new HashSet<string>();
        private HashSet<string> _metadataFilePaths = new HashSet<string>();

        

        private string[] _filesToKeep =
            {
                "data.db"
            };


        public JobProducer(ILogger<JobProducer> logger, IOptions<FolderSettings> folderSettings, PathHelper pathHelper, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _folderSettings = folderSettings;
            _pathHelper = pathHelper;
            _serviceProvider = serviceProvider;


        }

        public void RemoveNonExistingFromDisk(ISynchronizedQueue<IJob> queue)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var data = scope.ServiceProvider.GetService<IFilesData>();
                foreach (var folder in data.GetAll())
                {
                    _metadataFolderPaths.Add(_pathHelper.GetThumbnailPath(folder.Path));
                    foreach (var file in folder.Files)
                        _metadataFilePaths.Add(file.ThumbnailPath);
                }
            }

            ForEachFileSystemNode(_folderSettings.Value.ThumbnailsLogicalPrefix,
            (logicalPath, nodeType) =>
            {
                if (nodeType == NodeType.File && !_metadataFilePaths.Contains(logicalPath))
                {
                    var physicalPath = _pathHelper.GetPhysicalPath(logicalPath);
                    if (_filesToKeep.Contains(Path.GetFileName(physicalPath)))
                        return false;

                    queue.Enqueue(new RemoveFileFromDisk(_pathHelper.GetPhysicalPath(logicalPath)));
                    return false;
                }
                else if (nodeType == NodeType.Folder && !_metadataFolderPaths.Contains(logicalPath))
                {
                    queue.Enqueue(new RemoveFolderFromDisk(_pathHelper.GetPhysicalPath(logicalPath)));
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
                    var physicalPath = _pathHelper.GetPhysicalPath(logicalPath);
                    if (System.IO.File.GetLastWriteTime(physicalPath) != metadataByOriginalPath[logicalPath].ModificationTimestamp)
                        queue.Enqueue(new UpdateFile(logicalPath));

                    return false;
                }, false);
        }

        public void CreateFolders(ISynchronizedQueue<IJob> queue)
        {
            var metadataByOriginalPath = new Dictionary<string, Entities.Folder>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var data = scope.ServiceProvider.GetService<IFilesData>();

                foreach (var folder in data.GetAll())
                {
                    metadataByOriginalPath.Add(folder.Path, folder);
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
                        ForEachFileSystemNode(_pathHelper.JoinLogicalPaths(logicalPath, child), action);
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

            var physicalPath = _pathHelper.GetPhysicalPath(logicalPath);
            var fileAttr = System.IO.File.GetAttributes(physicalPath);
            if (fileAttr.HasFlag(FileAttributes.Directory))
            {
                return Directory.EnumerateFileSystemEntries(physicalPath).Select(x => Path.GetFileName(x));
            }
            else
                return null;
        }

        private enum NodeType
        {
            File,
            Folder
        }
    }


}
