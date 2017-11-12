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
        void Remove(File file);
        void Remove(Folder folder);
        void Add(File file);
        void Add(Folder folder);
        void SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
