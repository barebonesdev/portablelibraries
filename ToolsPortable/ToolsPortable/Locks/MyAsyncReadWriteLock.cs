using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable.Locks
{
    /// <summary>
    /// Can be released from any thread (doesn't have to be released from same thread that calls it)
    /// </summary>
    public class MyAsyncReadWriteLock
    {
        private abstract class MyLockQueueItem
        {
            private TaskCompletionSource<bool> _lockedCompletionSource;
            public Task LockedTask
            {
                get
                {
                    if (_lockedCompletionSource != null)
                    {
                        return _lockedCompletionSource.Task;
                    }
                    else
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            public MyAsyncReadWriteLock LockHost { get; private set; }

            public MyLockQueueItem(MyAsyncReadWriteLock lockHost, bool alreadyLocked)
            {
                LockHost = lockHost;
                if (!alreadyLocked)
                {
                    _lockedCompletionSource = new TaskCompletionSource<bool>();
                }
            }

            public void EnterLock()
            {
                _lockedCompletionSource?.TrySetResult(true);
            }

            public abstract void Release();

            protected void Finish()
            {
                LockHost.OnQueueItemFinished(this);
            }
        }

        private class MyReadLockQueueItem : MyLockQueueItem
        {
            public int NumberOfReadLocks { get; private set; } = 1;

            public MyReadLockQueueItem(MyAsyncReadWriteLock lockHost, bool alreadyLocked) : base(lockHost, alreadyLocked) { }

            public void Add()
            {
                NumberOfReadLocks++;
            }

            public override void Release()
            {
                lock (LockHost._lock)
                {
                    if (NumberOfReadLocks == 0)
                    {
                        throw new InvalidOperationException("Number of read locks was zero, cannot release more, bug in code.");
                    }

                    NumberOfReadLocks--;

                    if (NumberOfReadLocks == 0)
                    {
                        Finish();
                    }
                }
            }
        }

        private class MyWriteLockQueueItem : MyLockQueueItem
        {
            public MyWriteLockQueueItem(MyAsyncReadWriteLock lockHost, bool alreadyLocked) : base(lockHost, alreadyLocked) { }

            public override void Release()
            {
                Finish();
            }
        }

        private List<MyLockQueueItem> _queue = new List<MyLockQueueItem>();

        private class MyDisposableLock : IDisposable
        {
            private MyLockQueueItem _queueItem;
            public MyDisposableLock(MyLockQueueItem queueItem)
            {
                _queueItem = queueItem;
            }

            public void Dispose()
            {
                _queueItem.Release();
            }
        }

        private object _lock = new object();

        private void OnQueueItemFinished(MyLockQueueItem queueItem)
        {
            MyLockQueueItem nextToStart = null;
            lock (_lock)
            {
                if (_queue.FirstOrDefault() == queueItem)
                {
                    _queue.RemoveAt(0);
                    nextToStart = _queue.FirstOrDefault();
                }
                else
                {
                    // This probably shouldn't happen, but just in case...
                    _queue.Remove(queueItem);
                }
            }

            nextToStart?.EnterLock();
        }

        /// <summary>
        /// Can be disposed from any thread
        /// </summary>
        /// <returns></returns>
        public async Task<IDisposable> LockReadAsync(int millisecondsTimeout = int.MaxValue)
        {
            MyLockQueueItem queueItem;
            IDisposable answer;

            lock (_lock)
            {
                // If there's no items
                if (_queue.Count == 0)
                {
                    // Create a new read entry
                    var newEntry = new MyReadLockQueueItem(this, true);

                    // Add it to the queue
                    _queue.Add(newEntry);

                    // We're already locked, so just return instantly
                    return new MyDisposableLock(newEntry);
                }

                // If there's only one item, and it's another read, merge with it
                if (_queue.Count == 1 && _queue[0] is MyReadLockQueueItem)
                {
                    (_queue[0] as MyReadLockQueueItem).Add();

                    // We're already locked, so just return instantly
                    return new MyDisposableLock(_queue[0]);
                }

                // If there's a read at the end, merge with it
                else if (_queue.Last() is MyReadLockQueueItem)
                {
                    var last = _queue.Last() as MyReadLockQueueItem;
                    last.Add();

                    // Need to wait till this one starts
                    queueItem = last;
                    answer = new MyDisposableLock(last);
                }

                // Otherwise, the last item is a write, so we need to add new and wait
                else
                {
                    var newEntry = new MyReadLockQueueItem(this, alreadyLocked: false);

                    _queue.Add(newEntry);

                    queueItem = newEntry;
                    answer = new MyDisposableLock(newEntry);
                }
            }

            try
            {
                await TimeoutTask.Create(queueItem.LockedTask, millisecondsTimeout);
            }
            catch (TimeoutException)
            {
                queueItem.Release();
                throw new TimeoutException("Timeout exceeded and lock not established.");
            }
            return answer;
        }

        /// <summary>
        /// Can be disposed from any thread
        /// </summary>
        /// <returns></returns>
        public async Task<IDisposable> LockWriteAsync(int millisecondsTimeout = int.MaxValue)
        {
            MyLockQueueItem queueItem;
            IDisposable answer;

            lock (_lock)
            {
                // If there's no items
                if (_queue.Count == 0)
                {
                    // Create a new write entry
                    var newEntry = new MyWriteLockQueueItem(this, true);

                    // Add it to the queue
                    _queue.Add(newEntry);

                    // We're already locked, so just return instantly
                    return new MyDisposableLock(newEntry);
                }

                // Otherwise, we always just add for write locks
                else
                {
                    var newEntry = new MyWriteLockQueueItem(this, alreadyLocked: false);

                    _queue.Add(newEntry);

                    queueItem = newEntry;
                    answer = new MyDisposableLock(newEntry);
                }
            }

            try
            {
                await TimeoutTask.Create(queueItem.LockedTask, millisecondsTimeout);
            }
            catch (TimeoutException)
            {
                queueItem.Release();
                throw new TimeoutException("Timeout exceeded and lock not established.");
            }
            return answer;
        }
    }
}

