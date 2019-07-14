using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public abstract class PortableDispatcher
    {
        public void Run(Action codeToExecute)
        {
            var dontWait = RunAsync(codeToExecute);
        }

        /// <summary>
        /// Runs the action on the UI thread. If UI thread isn't available, throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="codeToExecute"></param>
        /// <returns></returns>
        public abstract Task RunAsync(Action codeToExecute);

        /// <summary>
        /// If UI thread is available, runs on UI thread. Otherwise falls back to running on current thread.
        /// </summary>
        /// <param name="codeToExecute"></param>
        /// <returns></returns>
        public Task RunOrFallbackToCurrentThreadAsync(Action codeToExecute)
        {
            try
            {
                return RunAsync(codeToExecute);
            }

            catch (InvalidOperationException)
            {
                codeToExecute();
                return Task.FromResult(true);
            }
        }

        public static PortableDispatcher GetCurrentDispatcher()
        {
            if (ObtainDispatcherFunction == null)
                throw new NullReferenceException("ObtainDispatcherFunction cannot be null");

            PortableDispatcher d = ObtainDispatcherFunction();

            if (d == null)
                throw new NullReferenceException("ObtainDispatcherFunction must return an initialized dispatcher.");

            return d;
        }

        /// <summary>
        /// Hosting app, like Android or UWP, must set this function (calling something like Window.Current.Dispatcher).
        /// </summary>
        public static Func<PortableDispatcher> ObtainDispatcherFunction { private get; set; }
    }
}
