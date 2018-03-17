using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShowPics.Cli.Jobs;
using ShowPics.Data.Abstractions;
using ShowPics.Entities;
using ShowPics.Utilities;
using ShowPics.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ShowPics.Cli
{
    public class ConversionRunner
    {
        private ILogger<ConversionRunner> _logger;
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



        public ConversionRunner(ILogger<ConversionRunner> logger, IOptions<FolderSettings> folderSettings, PathHelper pathHelper, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _folderSettings = folderSettings;
            _pathHelper = pathHelper;
            _serviceProvider = serviceProvider;
        }

        public void RemoveNonExistingFromDisk()
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
                var filesRemoved = 0;
                var foldersRemoved = 0;
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
                        _logger.LogInformation("Removing file '{FILE}' from disk.", physicalPath);
                        System.IO.File.Delete(physicalPath);
                        filesRemoved++;
                        return false;
                    }
                    else if (nodeType == NodeType.Folder && !_metadataFolderPaths.Contains(logicalPath))
                    {
                        var physicalPath = _pathHelper.GetPhysicalPath(logicalPath);
                        _logger.LogInformation("Removing folder '{FOLDER}' from disk.", physicalPath);
                        Directory.Delete(physicalPath, recursive: true);
                        foldersRemoved++;
                        return false;
                    }
                    return true;
                }, false);
                _logger.LogInformation("Visited {TOTAL} objects, folders removed: {FOLDERS}, files removed: {FILES}.", visited, foldersRemoved, filesRemoved);
            }

        }

        public void RemoveNonExistingFromDb()
        {
            _logger.LogInformation("Cleaning database");
            using (_logger.BeginScope("CleanDb"))
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
                    }, includingRoot: false);
                _logger.LogInformation("Found {FILES} files and {FOLDERS} folders", _originFilePaths.Count, _originFolderPaths.Count);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var data = scope.ServiceProvider.GetService<IFilesData>();
                    _logger.LogInformation("Loading database");
                    var topFolders = data.GetAll().Where(x => x.ParentId == null).ToList();
                    _logger.LogInformation("Iterating over database objects");
                    visited = 0;
                    lastProgressReport = DateTime.UtcNow;
                    RemoveNonExistingFromDbRecursively(topFolders, data, ref visited, ref lastProgressReport);
                    data.SaveChanges();
                }
            }
        }

        private void RemoveNonExistingFromDbRecursively(IEnumerable<Folder> folders, IFilesData data, ref int visited, ref DateTime lastProgressReport)
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
                    _logger.LogInformation($"Removing folder '{folder.Path}' from DB");
                    data.Remove(folder);
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
                        {
                            _logger.LogInformation($"Removing file '{file.Path}' from DB");
                            data.Remove(file);
                        }
                    }

                    RemoveNonExistingFromDbRecursively(folder.Children, data, ref visited, ref lastProgressReport);
                }
            }
        }

        public void CreateOrUpdateThumbs()
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
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                new CreateOrUpdateFile(logicalPath).Execute(scope.ServiceProvider);
                            }
                            return false;
                        }
                        var physicalPath = _pathHelper.GetPhysicalPath(logicalPath);
                        var fileTimestamp = System.IO.File.GetLastWriteTime(physicalPath);
                        // Drop any precision beyond seconds
                        if (new DateTime(fileTimestamp.Year, fileTimestamp.Month, fileTimestamp.Day, fileTimestamp.Hour, fileTimestamp.Minute, fileTimestamp.Second) != metadataByOriginalPath[logicalPath].ModificationTimestamp)
                        {
                            toUpdate++;
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                new CreateOrUpdateFile(logicalPath).Execute(scope.ServiceProvider);
                            }
                        }
                        return false;
                    }, false);
                _logger.LogInformation("Finished iterating over file system objects in original folders. Visited {visited}, to create {toCreate}, to update {toUpdate}, ", visited, toCreate, toUpdate);
            }
        }

        public void CreateFolders()
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
                var created = 0;
                var lastProgressReport = DateTime.UtcNow;
                ForEachFileSystemNode(_folderSettings.Value.OriginalsLogicalPrefix,
                    (logicalPath, nodeType) =>
                    {
                        if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                        {
                            _logger.LogInformation("Objects visited: {VISITED}. Created: {CREATED}.", visited, created);
                            lastProgressReport = DateTime.UtcNow;
                        }
                        visited++;
                        if (nodeType == NodeType.File)
                            return false;
                        if (!metadataByOriginalPath.ContainsKey(logicalPath))
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var data = scope.ServiceProvider.GetService<IFilesData>();
                                var physicalPath = _pathHelper.GetPhysicalPath(_pathHelper.GetThumbnailPath(logicalPath, false));
                                _logger.LogInformation("Creating folder {name}", physicalPath);
                                Directory.CreateDirectory(physicalPath);
                                var parentPath = _pathHelper.GetParentPath(logicalPath);
                                var parent = data.GetFolder(parentPath);
                                data.Add(new Folder()
                                {
                                    Name = _pathHelper.GetName(logicalPath),
                                    ParentId = parent?.Id,
                                    Path = logicalPath
                                });
                                data.SaveChanges();
                            }
                            created++;
                        }
                        return true;
                    }, false);
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

        public void Run()
        {
            RemoveNonExistingFromDb();
            RemoveNonExistingFromDisk();
            CreateFolders();
            CreateOrUpdateThumbs();
        }
    }
}
