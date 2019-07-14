using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    /// <summary>
    /// This worker queue is a more simple implementation of a worker queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleAsyncWorkerQueue<T>
    {
        internal event EventHandler OnAllCompleted;

        private class QueuedItem
        {
            public object MergeIdentifier { get; set; }

            public TaskCompletionSource<T> TaskCompletionSource { get; set; }

            public Func<Task<T>> WorkerFunction { get; set; }
        }

        private Queue<QueuedItem> _queue = new Queue<QueuedItem>();

        internal bool IsRunning
        {
            get
            {
                lock (this)
                {
                    return _queue.Count > 0;
                }
            }
        }

        public Task<T> QueueAsync(Func<Task<T>> workerFunction)
        {
            QueuedItem queuedItem = new QueuedItem()
            {
                TaskCompletionSource = new TaskCompletionSource<T>(),
                WorkerFunction = workerFunction
            };

            bool shouldStart = false;

            lock (this)
            {
                // Enqueue it
                _queue.Enqueue(queuedItem);

                // If this was the first, we have to start everything
                shouldStart = _queue.Count == 1;
            }

            if (shouldStart)
            {
                Start();
            }

            return queuedItem.TaskCompletionSource.Task;
        }

        /// <summary>
        /// Will merge with any pending tasks that haven't started yet and have the same mergeIdentifier.
        /// </summary>
        /// <param name="mergeIdentifier">Compared via object.Equals to determine if same instance and should be merged</param>
        /// <param name="workerFunction"></param>
        /// <returns></returns>
        public Task<T> QueueOrMergeAsync(object mergeIdentifier, Func<Task<T>> workerFunction, bool allowMergeWithAlreadyStarted = false)
        {
            if (mergeIdentifier == null)
            {
                throw new ArgumentNullException(nameof(mergeIdentifier));
            }

            QueuedItem queuedItem;

            bool shouldStart = false;

            lock (this)
            {
                if (allowMergeWithAlreadyStarted && _queue.Count > 0)
                {
                    var alreadyRunning = _queue.First();
                    if (alreadyRunning.MergeIdentifier != null && object.Equals(alreadyRunning.MergeIdentifier, mergeIdentifier))
                    {
                        // Return that task without modifying worker function, since it already started
                        return alreadyRunning.TaskCompletionSource.Task;
                    }
                }

                if (_queue.Count > 1)
                {
                    QueuedItem matching = _queue.Skip(1).FirstOrDefault(i => i.MergeIdentifier != null && object.Equals(i.MergeIdentifier, mergeIdentifier));

                    if (matching != null && matching != _queue.Peek())
                    {
                        // Update the worker function to the latest
                        matching.WorkerFunction = workerFunction;

                        // And return that task
                        return matching.TaskCompletionSource.Task;
                    }
                }

                // Otherwise need to schedule it
                queuedItem = new QueuedItem()
                {
                    TaskCompletionSource = new TaskCompletionSource<T>(),
                    WorkerFunction = workerFunction,
                    MergeIdentifier = mergeIdentifier
                };

                // Enqueue it
                _queue.Enqueue(queuedItem);

                // If this was the first, we have to start everything
                if (_queue.Count == 1)
                {
                    shouldStart = true;
                }
            }

            if (shouldStart)
            {
                Start();
            }

            return queuedItem.TaskCompletionSource.Task;
        }

        private async void Start()
        {
            bool shouldStartNext = false;
            do
            {
                QueuedItem queuedItem = null;
                lock (this)
                {
                    queuedItem = _queue.Peek();
                }

                try
                {
                    T answer = await queuedItem.WorkerFunction();
                    queuedItem.TaskCompletionSource.TrySetResult(answer);
                }
                catch (Exception ex)
                {
                    try
                    {
                        queuedItem.TaskCompletionSource.TrySetException(ex);
                    }
                    catch { }
                }

                lock (this)
                {
                    _queue.Dequeue();
                    shouldStartNext = _queue.Count > 0;
                }
            }
            while (shouldStartNext);

            OnAllCompleted?.Invoke(this, new EventArgs());
        }
    }

    public class SimpleAsyncWorkerQueue : SimpleAsyncWorkerQueue<bool>
    {
        public Task QueueAsync(Func<Task> workerFunction)
        {
            return base.QueueAsync(async delegate
            {
                await workerFunction();
                return true;
            });
        }

        public Task QueueOrMergeAsync(object mergeIdentifier, Func<Task> workerFunction)
        {
            return base.QueueOrMergeAsync(mergeIdentifier, async delegate
            {
                await workerFunction();
                return true;
            });
        }
    }

    public class MultipleChannelsWorkQueue<T>
    {
        private Dictionary<object, SimpleAsyncWorkerQueue<T>> _channels = new Dictionary<object, SimpleAsyncWorkerQueue<T>>();

        /// <summary>
        /// Queues or merges with existing.
        /// </summary>
        /// <param name="channelIdentifier">The identifier for determining which channel this should go under</param>
        /// <param name="mergeIdentifier">The merge identifier for mergining within a channel</param>
        /// <param name="workerFunction"></param>
        /// <returns></returns>
        public Task<T> QueueOrMergeAsync(object channelIdentifier, object mergeIdentifier, Func<Task<T>> workerFunction)
        {
            lock (this)
            {
                SimpleAsyncWorkerQueue<T> workerQueue;

                // If there's already a worker queue for this channel
                if (_channels.TryGetValue(channelIdentifier, out workerQueue))
                {
                    // By definition we know it's still running, so we just enqueue to it
                    return workerQueue.QueueOrMergeAsync(mergeIdentifier, workerFunction);
                }

                // Otherwise we need to create a worker queue channel
                workerQueue = new SimpleAsyncWorkerQueue<T>();
                workerQueue.OnAllCompleted += WorkerQueue_OnAllCompleted;

                // And add it to our channels
                _channels[channelIdentifier] = workerQueue;

                // Enqueue the work
                return workerQueue.QueueOrMergeAsync(mergeIdentifier, workerFunction);
            }
        }

        private void WorkerQueue_OnAllCompleted(object sender, EventArgs e)
        {
            lock (this)
            {
                // If our worker queue is done, we remove it
                var workerQueue = sender as SimpleAsyncWorkerQueue<T>;
                if (!workerQueue.IsRunning)
                {
                    var toRemove = _channels.First(i => i.Value == workerQueue);
                    _channels.Remove(toRemove.Key);
                }
            }
        }
    }

    public class MultipleChannelsWorkQueue
    {
        private MultipleChannelsWorkQueue<bool> _queue = new MultipleChannelsWorkQueue<bool>();

        /// <summary>
        /// Queues or merges with existing.
        /// </summary>
        /// <param name="channelIdentifier">The identifier for determining which channel this should go under</param>
        /// <param name="mergeIdentifier">The merge identifier for mergining within a channel</param>
        /// <param name="workerFunction"></param>
        /// <returns></returns>
        public Task QueueOrMergeAsync(object channelIdentifier, object mergeIdentifier, Func<Task> workerFunction)
        {
            return _queue.QueueOrMergeAsync(channelIdentifier, mergeIdentifier, async delegate
            {
                await workerFunction();
                return true;
            });
        }
    }

    public abstract class AsyncWorkerQueue<T>
    {
        public bool IsRunning { get; private set; }
        public bool IsAnotherQueued { get; private set; }
        public T QueuedData { get; private set; }

        /// <summary>
        /// Returns a task that represents when ALL are done, including anything subsequently queued while we were currently running.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task Enqueue(T data)
        {
            lock (this)
            {
                // If we're already running
                if (IsRunning)
                {
                    // If another is queued and we have data to merge, merge the data
                    if (IsAnotherQueued)
                    {
                        QueuedData = MergeData(data, QueuedData);
                    }

                    // Otherwise, queue another
                    else
                    {
                        QueuedData = data;
                        IsAnotherQueued = true;
                    }

                    // And stop
                    return Task.FromResult(true);
                }

                // Otherwise flag that we're about to start running
                IsRunning = true;
            }

            // And then start work
            return StartNext(data);
        }

        protected abstract T MergeData(T newData, T previouslyQueuedData);

        private async Task StartNext(T data)
        {
            try
            {
                await DoWorkAsync(data);
            }

            catch (Exception ex)
            {
                try
                {
                    OnException(ex, data);
                }
                catch { }
            }

            lock (this)
            {
                // If there's not another queued
                if (!IsAnotherQueued)
                {
                    // We're done
                    IsRunning = false;
                    return;
                }

                // Otherwise, we're still running but we clear the next queued
                IsAnotherQueued = false;
                data = QueuedData;
                QueuedData = default(T);
            }

            // And start again
            await StartNext(data);
        }

        protected abstract Task DoWorkAsync(T data);

        protected abstract void OnException(Exception ex, T data);
    }

    public abstract class AsyncMultiWorkerQueue<K, D>
    {
        private Dictionary<K, AsyncWorkerQueue<D>> _workerQueues = new Dictionary<K, AsyncWorkerQueue<D>>();

        /// <summary>
        /// Returns a task that represents when ALL queued items for that key are complete, including if another item gets queued while currently running.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task Enqueue(K key, D data)
        {
            lock (this)
            {
                AsyncWorkerQueue<D> workerQueue;

                // If there's already a worker queue
                if (_workerQueues.TryGetValue(key, out workerQueue))
                {
                    // By definition we know it's still running, so we just enqueue to it
                    return workerQueue.Enqueue(data);
                }

                // Otherwise we need to create a worker queue
                workerQueue = CreateWorkerQueue();

                // And add it to our queues
                _workerQueues[key] = workerQueue;

                // Enqueue the work
                Task queueDoneTask = workerQueue.Enqueue(data);

                // Register the cleanup handlers so that when it's done, it'll be removed from our queues
                RegisterOnCompletedCleanup(key, queueDoneTask, workerQueue);

                // And return the task
                return queueDoneTask;
            }
        }

        private async void RegisterOnCompletedCleanup(K key, Task queueDoneTask, AsyncWorkerQueue<D> queue)
        {
            await queueDoneTask;

            lock (this)
            {
                // Ensure nothing else was queued, since technically one could queue in before we establish the lock
                if (!queue.IsRunning)
                {
                    // Wasn't running, so remove
                    _workerQueues.Remove(key);
                }
            }
        }

        protected abstract AsyncWorkerQueue<D> CreateWorkerQueue();
    }
}
