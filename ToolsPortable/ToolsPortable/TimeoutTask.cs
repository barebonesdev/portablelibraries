using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    

    public class TimeoutTask
    {
        public static Task Create(Task originalTask, int millisecondTimeout)
        {
            return Create(AsParameterTask(originalTask), millisecondTimeout);
        }

        private static async Task<bool> AsParameterTask(Task normalTask)
        {
            await normalTask;
            return true;
        }

        public static Task<T> Create<T>(Task<T> originalTask, int millisecondTimeout)
        {
            return new TimeoutTaskImplementation<T>(originalTask, millisecondTimeout)._completionSource.Task;
        }

        private class TimeoutTaskImplementation<T>
        {
            public TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();
            private bool _isDone;

            public TimeoutTaskImplementation(Task<T> originalTask, int millisecondTimeout)
            {
                ConfigureTask(originalTask);
                ConfigureTimeout(millisecondTimeout);
            }

            private async void ConfigureTask(Task<T> originalTask)
            {
                try
                {
                    T answer = await originalTask;

                    if (!_isDone)
                    {
                        lock (this)
                        {
                            if (!_isDone)
                            {
                                _isDone = true;
                                _completionSource.TrySetResult(answer);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!_isDone)
                    {
                        lock (this)
                        {
                            if (!_isDone)
                            {
                                _isDone = true;
                                _completionSource.TrySetException(ex);
                            }
                        }
                    }
                }
            }

            private async void ConfigureTimeout(int millisecondTimeout)
            {
                if (millisecondTimeout == -1 || millisecondTimeout == int.MaxValue)
                {
                    return;
                }

                await Task.Delay(millisecondTimeout);

                if (!_isDone)
                {
                    lock (this)
                    {
                        if (!_isDone)
                        {
                            _isDone = true;
                            _completionSource.TrySetException(new TimeoutException("Timeout exceeded"));
                        }
                    }
                }
            }
        }
    }
}
