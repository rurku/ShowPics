using Microsoft.EntityFrameworkCore.Storage;
using ShowPics.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Data
{
    public class Transaction : ITransaction
    {
        private IDbContextTransaction _dbContextTransaction;

        public Transaction(IDbContextTransaction dbContextTransaction)
        {
            _dbContextTransaction = dbContextTransaction;
        }

        public void Commit()
        {
            _dbContextTransaction.Commit();
        }

        public void Dispose()
        {
            _dbContextTransaction.Dispose();
        }

        public void Rollback()
        {
            _dbContextTransaction.Rollback();
        }
    }
}
