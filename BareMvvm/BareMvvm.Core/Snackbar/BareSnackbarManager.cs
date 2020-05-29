using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

                DisplayedSnackbars.Clear();

                if (value != null)
                {
                    DisplayedSnackbars.Add(value);
                    HandleDecayingSnackbar(value);
                }
            }
        }

        public ObservableCollection<BareSnackbar> DisplayedSnackbars { get; private set; } = new ObservableCollection<BareSnackbar>();

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
                    if (CurrentSnackbar != newlyShownSnackbar)
                    {
                        return;
                    }

                    MoveToNext();
                }
            }
            catch { }
        }

        private void MoveToNext()
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

        public void Close(BareSnackbar bareSnackbar)
        {
            lock (this)
            {
                if (CurrentSnackbar == bareSnackbar)
                {
                    MoveToNext();
                }
                else if (_queuedSnackbars.Contains(bareSnackbar))
                {
                    _queuedSnackbars = new Queue<BareSnackbar>(_queuedSnackbars.Except(new BareSnackbar[] { bareSnackbar }));
                }
            }
        }
    }
}
