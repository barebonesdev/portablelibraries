using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable.Locks
{
    public class MyAsyncLock
    {
        private List<MyLockInstance> _locks = new List<MyLockInstance>();

        private class MyLockInstance : IDisposable
        {
            private MyAsyncLock _host;
            private TaskCompletionSource<IDisposable> _startCompletionSource;

            public MyLockInstance(MyAsyncLock host)
            {
                _host = host;
            }

            public static Task<IDisposable> CreateAsync(MyAsyncLock host, int millisecondTimeout)
            {
                var instance = new MyLockInstance(host);

                lock (host)
                {
                    host._locks.Add(instance);

                    // If there aren't any ahead of this
                    if (host._locks.Count == 1)
                    {
                        // Just start it immediately
                        return Task.FromResult<IDisposable>(instance);
                    }

                    // Otherwise, we need to set up a completion source
                    // which will be triggered after the one before it completes
                    instance._startCompletionSource = new TaskCompletionSource<IDisposable>();

                    if (millisecondTimeout != int.MaxValue)
                    {
                        instance.ConfigureTimeout(millisecondTimeout);
                    }

                    return instance._startCompletionSource.Task;
                }
            }

            private async void ConfigureTimeout(int millisecondTimeout)
            {
                await Task.Delay(millisecondTimeout);
                
                // If the task isn't completed yet, 
                if (!_startCompletionSource.Task.IsCompleted)
                {
                    lock (_host)
                    {
                        // Check that it's not completed again
                        if (!_startCompletionSource.Task.IsCompleted)
                        {
                            // If so, throw the exception
                            _startCompletionSource.TrySetException(new TimeoutException("Timeout exceeded and lock not established"));

                            // And dispose this
                            Dispose();
                        }
                    }
                }
            }

            private void Start()
            {
                if (_startCompletionSource != null)
                {
                    // If timeout occurred
                    if (_startCompletionSource.Task.IsFaulted)
                    {
                        // Do nothing
                        return;
                    }

                    // Otherwise we're starting the lock
                    else
                    {
                        var succeeded = _startCompletionSource.TrySetResult(this);
                        if (!succeeded)
                        {
                            Dispose();
                        }
                    }
                }
            }

            public void Dispose()
            {
                lock (_host)
                {
                    _host._locks.Remove(this);

                    var next = _host._locks.FirstOrDefault();
                    if (next != null)
                    {
                        next.Start();
                    }
                }
            }
        }

        public Task<IDisposable> LockAsync(int millisecondTimeout = int.MaxValue)
        {
            return MyLockInstance.CreateAsync(this, millisecondTimeout);
        }
    }
}
