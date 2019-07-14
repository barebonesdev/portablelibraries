using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ToolsPortable
{
    public abstract class BareTaskBase<T>
    {
        public BareTaskBase() { }

        public BareTaskBase(bool startInBackground) { StartInBackground = startInBackground; }

        protected bool StartInBackground { get; private set; }
        public delegate void TaskHandler(object sender, T result);

        private TaskHandler await;

        /// <summary>
        /// Only allows one event handler
        /// </summary>
        public event TaskHandler Await
        {
            add
            {
                await = (TaskHandler)Delegate.Combine(null, value);

                //execute the task
                Begin();
            }

            remove
            {
                await = (TaskHandler)Delegate.Remove(await, value);
                Cancel();
            }
        }

        public bool WasStarted { get; private set; }
        /// <summary>
        /// Typically, the task should be started by adding a listener to the Await event. But this can be called if you don't want to listen to the result.
        /// </summary>
        public void Begin()
        {
            if (!WasStarted)
            {
                WasStarted = true;

                if (StartInBackground && !IsBackground())
                {
                    var dontWait = System.Threading.Tasks.Task.Run(delegate { Start(); });
                }
                else
                    Start();
            }
        }

        private bool toUI;
        /// <summary>
        /// Only allows one event handler
        /// </summary>
        public event TaskHandler AwaitUI
        {
            add
            {
                await = (TaskHandler)Delegate.Combine(null, value);
                toUI = true;

                //execute the task
                Begin();
            }

            remove
            {
                await = (TaskHandler)Delegate.Remove(await, value);
                Cancel();
            }
        }

        protected abstract void Start();

        public bool IsDone { get; private set; }
        /// <summary>
        /// Extending class should call this when done. It automatically goes to UI thread if necessary.
        /// </summary>
        /// <param name="result"></param>
        protected virtual void Completed(T result)
        {
            if (IsCanceled)
                return;

            IsDone = true;

            if (await != null)
            {
                if (toUI && IsBackground())
                    BeginInvoke(delegate { await(this, result); AfterCompleted(result); });
                else
                {
                    await(this, result);
                    AfterCompleted(result);
                }
            }
        }

        /// <summary>
        /// Method that can be overridden to perform actions after task has been completed
        /// </summary>
        protected virtual void AfterCompleted(T result)
        {

        }

        protected abstract bool IsBackground();

        protected abstract void BeginInvoke(Action a);

        protected bool IsCanceled { get; private set; }
        public virtual void Cancel()
        {
            IsCanceled = true;
            IsDone = true;
        }
    }
}
