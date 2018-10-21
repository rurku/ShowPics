using ShowPics.Data.Abstractions;
using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;

namespace ShowPics.Data
{
    public class FilesData : IFilesData
    {
        private ShowPicsDbContext _dbContext;

        public FilesData(ShowPicsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(File file)
        {
            _dbContext.Files.Add(file);
        }

        public void Add(Folder folder)
        {
            _dbContext.Folders.Add(folder);
        }

        public ITransaction BeginTransaction()
        {
            return new Transaction(_dbContext.Database.BeginTransaction());
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new Transaction(await _dbContext.Database.BeginTransactionAsync(cancellationToken));
        }

        public ICollection<Folder> GetAll()
        {
            return _dbContext.Folders.Include(x => x.Files).ToListAsync().Result;
        }

        public ICollection<Folder> GetTopLevelFolders(int foldersDepth, int filesDepth)
        {
            if (foldersDepth < 0 || foldersDepth > 2)
                throw new ArgumentOutOfRangeException(nameof(foldersDepth), "Argument must be between 0 and 2");
            if (filesDepth < 0 || filesDepth > 2)
                throw new ArgumentOutOfRangeException(nameof(filesDepth), "Argument must be between 0 and 2");
            if (filesDepth > foldersDepth + 1)
                throw new ArgumentOutOfRangeException(nameof(filesDepth), $"Argument must not be greater than {nameof(foldersDepth)} + 1");

            IQueryable<Folder> foldersQueryable = _dbContext.Folders;
            if (foldersDepth > 0)
            {
                var includable = foldersQueryable.Include(x => x.Children);
                if (foldersDepth > 1)
                {
                    includable = includable.ThenInclude(x => x.Children);
                }
                foldersQueryable = includable;
            }
            if (filesDepth > 0)
            {
                foldersQueryable = foldersQueryable.Include(x => x.Files);

                if (filesDepth > 1)
                {
                    foldersQueryable = foldersQueryable.Include(x => x.Children).ThenInclude(x => x.Files);
                }
            }

            var result = foldersQueryable.Where(x => x.ParentId == null).ToListAsync().Result;
            return result;
        }

        public Folder GetFolder(string logicalPath, int foldersDepth, int filesDepth)
        {
            if (foldersDepth < 0 || foldersDepth > 2)
                throw new ArgumentOutOfRangeException(nameof(foldersDepth), "Argument must be between 0 and 2");
            if (filesDepth < 0 || filesDepth > 2)
                throw new ArgumentOutOfRangeException(nameof(filesDepth), "Argument must be between 0 and 2");
            if (filesDepth > foldersDepth + 1)
                throw new ArgumentOutOfRangeException(nameof(filesDepth), $"Argument must not be greater than {nameof(foldersDepth)} + 1");

            IQueryable<Folder> foldersQueryable = _dbContext.Folders;
            if (foldersDepth > 0)
            {
                var includable = foldersQueryable.Include(x => x.Children);
                if (foldersDepth > 1)
                {
                    includable = includable.ThenInclude(x => x.Children);
                }
                foldersQueryable = includable;
            }
            if (filesDepth > 0)
            {
                foldersQueryable = foldersQueryable.Include(x => x.Files);

                if (filesDepth > 1)
                {
                    foldersQueryable = foldersQueryable.Include(x => x.Children).ThenInclude(x => x.Files);
                }
            }

            var result = foldersQueryable.SingleOrDefaultAsync(x => x.Path == logicalPath).Result;
            return result;
        }

        public async Task<ICollection<Folder>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _dbContext.Folders.Include(x => x.Files).ToListAsync(cancellationToken);
        }

        public File GetFile(string logicalPath)
        {
            return _dbContext.Files.SingleOrDefaultAsync(x => x.Path == logicalPath).Result;
        }

        public Folder GetFolder(string logicalPath)
        {
            return _dbContext.Folders.SingleOrDefaultAsync(x => x.Path == logicalPath).Result;
        }

        public void Remove(File file)
        {
            _dbContext.Files.Remove(file);
        }

        public void Remove(Folder folder)
        {
            _dbContext.Folders.Remove(folder);
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
