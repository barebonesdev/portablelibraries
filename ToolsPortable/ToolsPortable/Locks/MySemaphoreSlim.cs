using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class MySemaphoreSlim
    {
        public class MyDisposableSemaphoreSlim : IDisposable
        {
            private MySemaphoreSlim _semaphore;
            public MyDisposableSemaphoreSlim(MySemaphoreSlim semaphore, int millisecondTimeout)
            {
                _semaphore = semaphore;
                if (!semaphore.Wait(millisecondTimeout))
                    throw new TimeoutException("Timeout for locker has been reached and lock hasn't been established.");
            }

            public void Dispose()
            {
                _semaphore.Release();
            }
        }

        private SemaphoreSlim _semaphore;

        public MySemaphoreSlim(int maximumCount, string name = null)
        {
            _semaphore = new SemaphoreSlim(maximumCount, maximumCount);
        }

        /// <summary>
        /// Waits on the current thread
        /// </summary>
        public bool Wait(int millisecondTimeout)
        {
            return _semaphore.Wait(millisecondTimeout);
        }

        public void Release()
        {
            _semaphore.Release();
        }

        public void Release(int releaseCount)
        {
            _semaphore.Release(releaseCount);
        }

        public IDisposable Lock(int millisecondTimeout)
        {
            return new MyDisposableSemaphoreSlim(this, millisecondTimeout);
        }
    }
}
