using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class WeakEventHandler : WeakEventHandler<EventArgs>
    {
#if DEBUG
        public static Action ObjectDisposedAction { get; set; }

        public static void InvokeObjectDisposedAction()
        {
            ObjectDisposedAction?.Invoke();
        }
#endif

        public WeakEventHandler(EventHandler<EventArgs> callback) : base(callback) { }
    }

    // http://paulstovell.com/blog/weakevents (this implementation actually doesn't work)
    // https://code.logos.com/blog/2008/08/event_subscription_using_weak_references.html
    public class WeakEventHandler<TEventArgs>
    {
        private WeakReference _targetReference;
        private MethodInfo _methodInfo;

        public WeakEventHandler(EventHandler<TEventArgs> callback)
        {
            // Since we originally weak referenced the event handler, the event handler will obviously be disposed unless the calling
            // subscriber holds a reference to it.

            // We'll hold a weak reference to the subscriber target
            _targetReference = new WeakReference(callback.Target);

            // And also obtain the actual method info
            _methodInfo = callback.GetMethodInfo();
        }

        public void Handler(object sender, TEventArgs e)
        {
            if (_targetReference != null)
            {
                object target = _targetReference.Target;
                if (target != null)
                {
#if ANDROID
                    try
                    {
#endif
                        _methodInfo.Invoke(target, new object[] { sender, e });
                        return;
#if ANDROID
                    }

                    catch (Exception ex)
                    {
                        // Sometimes it can be contained in a TargetInvocationException, and I think the ObjectDisposed is the inner exception.
                        // We'll just check all exception types.
                        // On Android, this means the item ultimately should be garbaged collected, but it does it lazily.
                        // The Java object has already been collected. So we'll let the cleanup logic execute below.
                        if (!ContainsObjectDisposedException(ex))
                        {
                            throw ex;
                        }
#if DEBUG
                        else
                        {
                            WeakEventHandler.ObjectDisposedAction();
                        }
#endif
                    }
#endif
                }
            }

            // Otherwise clean up
            _targetReference = null;
            _methodInfo = null;
        }

        private static bool ContainsObjectDisposedException(Exception ex)
        {
            if (ex is ObjectDisposedException)
            {
                return true;
            }

            if (ex is ArgumentException && ex.Message != null && ex.Message.StartsWith("Handle must be valid."))
            {
                return true;
            }

            if (ex.InnerException != null)
            {
                return ContainsObjectDisposedException(ex.InnerException);
            }

            return false;
        }
    }
}
