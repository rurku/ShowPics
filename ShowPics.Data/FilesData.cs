using ShowPics.Data.Abstractions;
using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ICollection<Folder>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _dbContext.Folders.Include(x => x.Files).ToListAsync(cancellationToken);
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
