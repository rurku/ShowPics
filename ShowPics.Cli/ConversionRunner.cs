using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShowPics.Data.Abstractions;
using ShowPics.Entities;
using ShowPics.Utilities;
using ShowPics.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShowPics.Cli
{
    public class ConversionRunner
    {
        private ILogger<ConversionRunner> _logger;
        private IOptions<FolderSettings> _folderSettings;
        private readonly PathHelper _pathHelper;
        private IServiceProvider _serviceProvider;

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
            var metadataFolderPaths = new HashSet<string>();
            var metadataFilePaths = new HashSet<string>();

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
                        metadataFolderPaths.Add(_pathHelper.GetThumbnailPath(folder.Path, false));
                        foreach (var file in folder.Files)
                            metadataFilePaths.Add(file.ThumbnailPath);
                    }
                    _logger.LogInformation("Found {FOLDERS} folders and {FILES} files in db", metadataFolderPaths.Count, metadataFilePaths.Count);
                }

                _logger.LogInformation("Iterating over file system objects in thumbnails folder");
                var visited = 0;
                var filesRemoved = 0;
                var foldersRemoved = 0;
                var lastProgressReport = DateTime.UtcNow;
                foreach(var item in GetFileSystemNodeEnumerable(_folderSettings.Value.ThumbnailsLogicalPrefix, includingRoot: false))
                {
                    if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                    {
                        _logger.LogInformation("Objects visited: {VISITED}", visited);
                        lastProgressReport = DateTime.UtcNow;
                    }

                    visited++;
                    if (item.NodeType == NodeType.File && !metadataFilePaths.Contains(item.LogicalPath))
                    {
                        var physicalPath = _pathHelper.GetPhysicalPath(item.LogicalPath);
                        if (!_filesToKeep.Contains(Path.GetFileName(physicalPath)))
                        {
                            _logger.LogInformation("Removing file '{FILE}' from disk.", physicalPath);
                            System.IO.File.Delete(physicalPath);
                            filesRemoved++;
                        }
                    }
                    else if (item.NodeType == NodeType.Folder && !metadataFolderPaths.Contains(item.LogicalPath))
                    {
                        var physicalPath = _pathHelper.GetPhysicalPath(item.LogicalPath);
                        _logger.LogInformation("Removing folder '{FOLDER}' from disk.", physicalPath);
                        Directory.Delete(physicalPath, recursive: true);
                        foldersRemoved++;
                        item.EnumerateContents = false;
                    }
                }
                _logger.LogInformation("Visited {TOTAL} objects, folders removed: {FOLDERS}, files removed: {FILES}.", visited, foldersRemoved, filesRemoved);
            }

        }

        private (HashSet<string> folderPaths, HashSet<string> filePaths) ScanOriginFolders()
        {
            var originFilePaths = new HashSet<string>();
            var originFolderPaths = new HashSet<string>();

            _logger.LogInformation("Iterating over file system objects in original folders");
            var visited = 0;
            var lastProgressReport = DateTime.UtcNow;
            foreach (var item in GetFileSystemNodeEnumerable(_folderSettings.Value.OriginalsLogicalPrefix, includingRoot: false))
            {
                if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                {
                    _logger.LogInformation("Objects visited: {VISITED}", visited);
                    lastProgressReport = DateTime.UtcNow;
                }
                visited++;
                if (item.NodeType == NodeType.File)
                    originFilePaths.Add(item.LogicalPath);
                else
                    originFolderPaths.Add(item.LogicalPath);
            }
            _logger.LogInformation("Found {FILES} files and {FOLDERS} folders", originFilePaths.Count, originFolderPaths.Count);
            return (originFolderPaths, originFilePaths);
        }

        public void RemoveNonExistingFromDb()
        {
            var (originFolderPaths, originFilePaths) = ScanOriginFolders();
            _logger.LogInformation("Cleaning database");
            using (_logger.BeginScope("CleanDb"))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var data = scope.ServiceProvider.GetService<IFilesData>();
                    _logger.LogInformation("Loading database");
                    var topFolders = data.GetAll().Where(x => x.ParentId == null).ToList();
                    _logger.LogInformation("Iterating over database objects");
                    var visited = 0;
                    var lastProgressReport = DateTime.UtcNow;
                    RemoveNonExistingFromDbRecursively(topFolders, data, originFolderPaths, originFilePaths, ref visited, ref lastProgressReport);
                    data.SaveChanges();
                }
            }
        }

        private void RemoveNonExistingFromDbRecursively(IEnumerable<Folder> folders, IFilesData data, HashSet<string> originFolderPaths, HashSet<string> originFilePaths, ref int visited, ref DateTime lastProgressReport)
        {
            foreach (var folder in folders)
            {
                if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                {
                    _logger.LogInformation("Objects visited: {VISITED}", visited);
                    lastProgressReport = DateTime.UtcNow;
                }
                visited++;
                if (!originFolderPaths.Contains(folder.Path))
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
                        if (!originFilePaths.Contains(file.Path))
                        {
                            _logger.LogInformation($"Removing file '{file.Path}' from DB");
                            data.Remove(file);
                        }
                    }

                    RemoveNonExistingFromDbRecursively(folder.Children, data, originFolderPaths, originFilePaths, ref visited, ref lastProgressReport);
                }
            }
        }

        private bool IsFormatSupported(string logicalPath)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var creatorFactory = scope.ServiceProvider.GetService<Func<string, IThumbnailCreator>>();
                var thumbnailCreator = creatorFactory(logicalPath);
                return thumbnailCreator.IsFormatSupported();
            }
        }
        

        public IEnumerable<string> GetFilesToCreateOrUpdateEnumerable()
        {
            _logger.LogInformation("Enumerating files to create or update");
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

            foreach (var item in GetFileSystemNodeEnumerable(_folderSettings.Value.OriginalsLogicalPrefix, includingRoot: false))
            {
                if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                {
                    _logger.LogInformation("Objects visited: {VISITED}. To create: {toCreate}, to update: {toUpdate}.", visited, toCreate, toUpdate);
                    lastProgressReport = DateTime.UtcNow;
                }
                visited++;

                if (item.NodeType != NodeType.Folder && IsFormatSupported(item.LogicalPath))
                {
                    if (!metadataByOriginalPath.ContainsKey(item.LogicalPath))
                    {
                        toCreate++;
                        yield return item.LogicalPath;
                    }
                    else
                    {
                        var physicalPath = _pathHelper.GetPhysicalPath(item.LogicalPath);
                        var fileTimestamp = System.IO.File.GetLastWriteTime(physicalPath);
                        // Drop any precision beyond seconds
                        if (new DateTime(fileTimestamp.Year, fileTimestamp.Month, fileTimestamp.Day, fileTimestamp.Hour, fileTimestamp.Minute, fileTimestamp.Second) != metadataByOriginalPath[item.LogicalPath].ModificationTimestamp)
                        {
                            toUpdate++;
                            yield return item.LogicalPath;
                        }
                    }
                }
            }
            _logger.LogInformation("Finished iterating over file system objects in original folders. Visited {visited}, to create {toCreate}, to update {toUpdate}, ", visited, toCreate, toUpdate);
        }

        public void CreateOrUpdateThumbs()
        {
            _logger.LogInformation("Creating/updating thumbnails and metadata");
            using (_logger.BeginScope("CreateOrUpdateThumbs"))
            {
                Parallel.ForEach(GetFilesToCreateOrUpdateEnumerable(), new ParallelOptions() { MaxDegreeOfParallelism = _folderSettings.Value.ConversionThreads }
                    , logicalPath =>
                    {
                        using (_logger.BeginScope("Thread {ID}", Thread.CurrentThread.ManagedThreadId))
                        {
                            _logger.LogInformation("Creating or updating file '{LOGICAL_PATH}'", logicalPath);
                            try
                            {
                                using (var scope = _serviceProvider.CreateScope())
                                {
                                    var creatorFactory = scope.ServiceProvider.GetService<Func<string, IThumbnailCreator>>();
                                    var thumbnailCreator = creatorFactory(logicalPath);
                                    thumbnailCreator.CreateOrUpdateThumbnail();
                                }
                                _logger.LogInformation("Creating or updating file '{LOGICAL_PATH}' complete!", logicalPath);
                            }
                            catch (ImageProcessingException e)
                            {
                                _logger.LogWarning(e, "Non-fatal error processing file '{LOGICAL_PATH}'.", logicalPath);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Error processing file '{LOGICAL_PATH}'.", logicalPath);
                                throw;
                            }
                            if (_folderSettings.Value.ForceGCAfterEachImage)
                                ForceGC();
                        }
                    });
            }
        }

        private void ForceGC()
        {
            _logger.LogDebug("Forcing garbage collection");
            var watch = new Stopwatch();
            watch.Start();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            _logger.LogDebug("GC completed in {milliseconds}", watch.ElapsedMilliseconds);
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
                foreach (var item in GetFileSystemNodeEnumerable(_folderSettings.Value.OriginalsLogicalPrefix, includingRoot: false))
                {
                    if (DateTime.UtcNow - lastProgressReport > new TimeSpan(0, 0, 10))
                    {
                        _logger.LogInformation("Objects visited: {VISITED}. Created: {CREATED}.", visited, created);
                        lastProgressReport = DateTime.UtcNow;
                    }
                    visited++;
                    if (item.NodeType == NodeType.Folder && !metadataByOriginalPath.ContainsKey(item.LogicalPath))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var data = scope.ServiceProvider.GetService<IFilesData>();
                            var physicalPath = _pathHelper.GetPhysicalPath(_pathHelper.GetThumbnailPath(item.LogicalPath, false));
                            _logger.LogInformation("Creating folder {name}", physicalPath);
                            Directory.CreateDirectory(physicalPath);
                            var parentPath = _pathHelper.GetParentPath(item.LogicalPath);
                            var parent = data.GetFolder(parentPath);
                            data.Add(new Folder()
                            {
                                Name = _pathHelper.GetName(item.LogicalPath),
                                ParentId = parent?.Id,
                                Path = item.LogicalPath
                            });
                            data.SaveChanges();
                        }
                        created++;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an IEnumerable that can be used to enumerate logical file system nodes
        ///     <para>
        ///     If the returned IEnumerable is used in a foreach loop, then it's possible to skip enumerating folder contents by setting EnumerateContents to false on the folder node.
        ///     </para>
        /// </summary>
        /// <param name="logicalPath">Logical folder path to enumerate</param>
        /// <param name="includingRoot">Whether to include the enumeration root node in the results</param>
        /// <returns>IEnumerable of file system nodes</returns>
        private IEnumerable<FileSystemNodeEnumerationItem> GetFileSystemNodeEnumerable(string logicalPath, bool includingRoot = true)
        {
            var children = GetChildren(logicalPath);
            if (children != null)
            {
                var enumerationItem = new FileSystemNodeEnumerationItem() { LogicalPath = logicalPath, NodeType = NodeType.Folder };
                if (includingRoot)
                    yield return enumerationItem;
                if (enumerationItem.EnumerateContents)
                    foreach (var child in children)
                    {
                        foreach (var item in GetFileSystemNodeEnumerable(_pathHelper.JoinLogicalPaths(logicalPath, child)))
                            yield return item;
                    }
            }
            else
            {
                yield return new FileSystemNodeEnumerationItem() { LogicalPath = logicalPath, NodeType = NodeType.File };
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

        public void Run()
        {
            RemoveNonExistingFromDb();
            RemoveNonExistingFromDisk();
            CreateFolders();
            CreateOrUpdateThumbs();
        }

        private class FileSystemNodeEnumerationItem
        {
            public string LogicalPath { get; set; }
            public NodeType NodeType { get; set; }
            public bool EnumerateContents { get; set; } = true;
        }

        private enum NodeType
        {
            File,
            Folder
        }
    }
}
