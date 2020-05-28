using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace BareMvvm.Core.Snackbar
{
    public class BareSnackbarManager : BindableBase
    {
        private Queue<BareSnackbar> _queuedSnackbars = new Queue<BareSnackbar>();

        private BareSnackbar _currentSnackbar;
        public BareSnackbar CurrentSnackbar
        {
            get => _currentSnackbar;
            private set
            {
                SetProperty(ref _currentSnackbar, value, nameof(CurrentSnackbar));

                if (value != null)
                {
                    HandleDecayingSnackbar(value);
                }
            }
        }

        public void Show(BareSnackbar snackbar)
        {
            lock (this)
            {
                if (CurrentSnackbar == null)
                {
                    CurrentSnackbar = snackbar;
                }

                else
                {
                    _queuedSnackbars.Enqueue(snackbar);
                }
            }
        }

        private async void HandleDecayingSnackbar(BareSnackbar newlyShownSnackbar)
        {
            try
            {
                await Task.Delay(newlyShownSnackbar.Duration);

                lock (this)
                {
                    if (_queuedSnackbars.Count > 0)
                    {
                        CurrentSnackbar = _queuedSnackbars.Dequeue();
                    }

                    else
                    {
                        CurrentSnackbar = null;
                    }
                }
            }
            catch { }
        }
    }
}
