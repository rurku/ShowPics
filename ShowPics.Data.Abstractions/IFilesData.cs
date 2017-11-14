using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShowPics.Data.Abstractions
{
    public interface IFilesData
    {
        Task<ICollection<Folder>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        ICollection<Folder> GetAll();
        void Remove(File file);
        void Remove(Folder folder);
        void Add(File file);
        void Add(Folder folder);
        Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        void SaveChanges();
        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));
        ITransaction BeginTransaction();
        Folder GetFolder(string logicalPath);
        File GetFile(string logicalPath);
    }
}
