using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Cli
{
    public interface ISynchronizedQueue<T>
    {
        void Enqueue(T item);
        T Dequeue();
    }
}
