using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Data.Abstractions
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
