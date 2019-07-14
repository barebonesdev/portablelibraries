using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class MySemaphore
    {
        public class MyDisposableSemaphore : IDisposable
        {
            private MySemaphore _semaphore;
            public MyDisposableSemaphore(MySemaphore semaphore)
            {
                _semaphore = semaphore;
                semaphore.WaitOne();
            }

            public void Dispose()
            {
                _semaphore.Release();
            }
        }

        private Semaphore _semaphore;

        public MySemaphore(int maximumCount, string name = null)
        {
            bool createdNew;

            _semaphore = new Semaphore(0, maximumCount, name, out createdNew);
            
            // If it's a new one, then we'll allocate everything so that it's currently empty and anyone can enter
            if (createdNew)
                _semaphore.Release(maximumCount);
        }

        /// <summary>
        /// Spins off a new thread to wait on, so UI won't be blocked.
        /// </summary>
        /// <returns></returns>
        public async Task WaitOneAsync()
        {
            await Task.Run(new Action(WaitOne));
        }

        /// <summary>
        /// Waits on the current thread
        /// </summary>
        public void WaitOne()
        {
            _semaphore.WaitOne();
        }

        public void Release()
        {
            _semaphore.Release();
        }

        public void Release(int releaseCount)
        {
            _semaphore.Release(releaseCount);
        }

        public MyDisposableSemaphore Lock()
        {
            return new MyDisposableSemaphore(this);
        }
    }
}
