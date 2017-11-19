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
using System.Text.RegularExpressions;

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
            _logger.LogInformation("Cleaning thumbnails directory");
            using (_logger.BeginScope("CleanThumbnails"))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var data = scope.ServiceProvider.GetService<IFilesData>();
                    _logger.LogInformation("Loading database");
                    var allObjects = data.GetAll();
                    _logger.LogInformation("Building db objects lookup table");
                    foreach (var folder in allObjects)
                    {
                        _metadataFolderPaths.Add(_pathHelper.GetThumbnailPath(folder.Path, false));
                        foreach (var file in folder.Files)
                            _metadataFilePaths.Add(file.ThumbnailPath);
                    }
                    _logger.LogInformation("Found {FOLDERS} folders and {FILES} files in db", _metadataFolderPaths.Count, _metadataFilePaths.Count);
                }

                _logger.LogInformation("Iterating over file system objects in thumbnails folder");
                var visited = 0;
                var filesToRemove = 0;
                var foldersToRemove = 0;
                var lastProgressReport = DateTime.UtcNow;
                ForEachFileSystemNode(_folderSettings.Value.ThumbnailsLogicalPrefix,
                (logicalPath, nodeType) =>
                {
                    if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                    {
                        _logger.LogInformation("Objects visited: {VISITED}", visited);
                        lastProgressReport = DateTime.UtcNow;
                    }

                    visited++;
                    if (nodeType == NodeType.File && !_metadataFilePaths.Contains(logicalPath))
                    {
                        var physicalPath = _pathHelper.GetPhysicalPath(logicalPath);
                        if (_filesToKeep.Contains(Path.GetFileName(physicalPath)))
                            return false;
                        filesToRemove++;
                        queue.Enqueue(new RemoveFileFromDisk(_pathHelper.GetPhysicalPath(logicalPath)));
                        return false;
                    }
                    else if (nodeType == NodeType.Folder && !_metadataFolderPaths.Contains(logicalPath))
                    {
                        foldersToRemove++;
                        queue.Enqueue(new RemoveFolderFromDisk(_pathHelper.GetPhysicalPath(logicalPath)));
                        return false;
                    }
                    return true;
                }, false);
                _logger.LogInformation("Visited {TOTAL} objects, folders to remove: {FOLDERS}, files to remove: {FILES}.", visited, foldersToRemove, filesToRemove);
            }

        }

        public void RemoveNonExistingFromDb(ISynchronizedQueue<IJob> queue)
        {
            _logger.LogInformation("Cleaning database");
            using (_logger.BeginScope("CleanThumbnails"))
            {
                _logger.LogInformation("Iterating over file system objects in original folders");
                var visited = 0;
                var lastProgressReport = DateTime.UtcNow;
                ForEachFileSystemNode(_folderSettings.Value.OriginalsLogicalPrefix,
                (logicalPath, nodeType) =>
                {
                    if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                    {
                        _logger.LogInformation("Objects visited: {VISITED}", visited);
                        lastProgressReport = DateTime.UtcNow;
                    }
                    visited++;
                    if (nodeType == NodeType.File)
                        _originFilePaths.Add(logicalPath);
                    else
                        _originFolderPaths.Add(logicalPath);
                    return true;
                }, false);
                _logger.LogInformation("Found {FILES} files and {FOLDERS} folders", _originFilePaths.Count, _originFolderPaths.Count);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var data = scope.ServiceProvider.GetService<IFilesData>();
                    _logger.LogInformation("Loading database");
                    var topFolders = data.GetAll().Where(x => x.ParentId == null).ToList();
                    _logger.LogInformation("Iterating over database objects");
                    visited = 0;
                    lastProgressReport = DateTime.UtcNow;
                    RemoveNonExistingFromDbRecursively(topFolders, queue, ref visited, ref lastProgressReport);
                }
            }
        }

        public void CreateOrUpdateThumbs(ISynchronizedQueue<IJob> queue)
        {
            _logger.LogInformation("Creating thumbnails");
            using (_logger.BeginScope("CreateOrUpdateThumbs"))
            {
                var metadataByOriginalPath = new Dictionary<string, Entities.File>();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var data = scope.ServiceProvider.GetService<IFilesData>();
                    _logger.LogInformation("Loading database");
                    foreach (var folder in data.GetAll())
                    {
                        foreach (var file in folder.Files)
                            metadataByOriginalPath.Add(file.Path, file);
                    }
                }

                _logger.LogInformation("Iterating over file system objects in original folders");
                var visited = 0;
                var toCreate = 0;
                var toUpdate = 0;
                var lastProgressReport = DateTime.UtcNow;

                ForEachFileSystemNode(_folderSettings.Value.OriginalsLogicalPrefix,
                    (logicalPath, nodeType) =>
                    {
                        if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                        {
                            _logger.LogInformation("Objects visited: {VISITED}. To create: {toCreate}, to update: {toUpdate}.", visited, toCreate, toUpdate);
                            lastProgressReport = DateTime.UtcNow;
                        }
                        visited++;
                        if (nodeType == NodeType.Folder)
                            return true;
                        if (!CreateOrUpdateFile.IsFormatSupported(logicalPath))
                            return false;
                        if (!metadataByOriginalPath.ContainsKey(logicalPath))
                        {
                            toCreate++;
                            queue.Enqueue(new CreateOrUpdateFile(logicalPath));
                            return false;
                        }
                        var physicalPath = _pathHelper.GetPhysicalPath(logicalPath);
                        if (System.IO.File.GetLastWriteTime(physicalPath) != metadataByOriginalPath[logicalPath].ModificationTimestamp)
                        {
                            toUpdate++;
                            queue.Enqueue(new CreateOrUpdateFile(logicalPath));
                        }
                        return false;
                    }, false);
                _logger.LogInformation("Finished iterating over file system objects in original folders. Visited {visited}, to create {toCreate}, to update {toUpdate}, ", visited, toCreate, toUpdate);
            }
        }

        public void CreateFolders(ISynchronizedQueue<IJob> queue)
        {
            _logger.LogInformation("Creating thumbnail folders");
            using (_logger.BeginScope("CreateFolders"))
            {
                var metadataByOriginalPath = new Dictionary<string, Entities.Folder>();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var data = scope.ServiceProvider.GetService<IFilesData>();
                    _logger.LogInformation("Loading database");
                    foreach (var folder in data.GetAll())
                    {
                        metadataByOriginalPath.Add(folder.Path, folder);
                    }
                }

                _logger.LogInformation("Iterating over file system objects in original folders");
                var visited = 0;
                var toCreate = 0;
                var lastProgressReport = DateTime.UtcNow;
                ForEachFileSystemNode(_folderSettings.Value.OriginalsLogicalPrefix,
                    (logicalPath, nodeType) =>
                    {
                        if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                        {
                            _logger.LogInformation("Objects visited: {VISITED}. To create: {toCreate}.", visited, toCreate);
                            lastProgressReport = DateTime.UtcNow;
                        }
                        visited++;
                        if (nodeType == NodeType.File)
                            return false;
                        if (!metadataByOriginalPath.ContainsKey(logicalPath))
                        {
                            toCreate++;
                            queue.Enqueue(new CreateFolder(logicalPath));
                        }
                        return true;
                    }, false);
            }
        }

        private void RemoveNonExistingFromDbRecursively(IEnumerable<Folder> folders, ISynchronizedQueue<IJob> queue, ref int visited, ref DateTime lastProgressReport)
        {
            foreach (var folder in folders)
            {
                if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                {
                    _logger.LogInformation("Objects visited: {VISITED}", visited);
                    lastProgressReport = DateTime.UtcNow;
                }
                visited++;
                if (!_originFolderPaths.Contains(folder.Path))
                {
                    queue.Enqueue(new RemoveFolderFromDb(folder));
                }
                else
                {
                    foreach (var file in folder.Files)
                    {
                        if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                        {
                            _logger.LogInformation("Objects visited: {VISITED}", visited);
                            lastProgressReport = DateTime.UtcNow;
                        }
                        visited++;
                        if (!_originFilePaths.Contains(file.Path))
                            queue.Enqueue(new RemoveFileFromDb(file));
                    }

                    RemoveNonExistingFromDbRecursively(folder.Children, queue, ref visited, ref lastProgressReport);
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
