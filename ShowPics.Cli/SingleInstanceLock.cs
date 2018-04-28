using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ShowPics.Cli
{
    public interface ISingleInstanceLock
    {
        IDisposable Lock(string name);
    }

    public class SingleInstanceLock : ISingleInstanceLock
    {
        public IDisposable Lock(string name)
        {
            return new SingleInstanceLockContext(name);
        }
    }

    internal class SingleInstanceLockContext : IDisposable
    {
        private Mutex _mutex;
        public SingleInstanceLockContext(string name)
        {
            var mutexName = $"Global\\{Assembly.GetEntryAssembly().GetName().Name}_{Hash(name)}";
            _mutex = new Mutex(false, mutexName);
            if (!_mutex.WaitOne(0))
                throw new AnotherInstanceRunningException($"Could not acquire mutex named {mutexName}");
        }

        public void Dispose()
        {
            _mutex.ReleaseMutex();
        }

        private string Hash(string text)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }

    [Serializable]
    internal class AnotherInstanceRunningException : Exception
    {
        public AnotherInstanceRunningException()
        {
        }

        public AnotherInstanceRunningException(string message) : base(message)
        {
        }

        public AnotherInstanceRunningException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AnotherInstanceRunningException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
