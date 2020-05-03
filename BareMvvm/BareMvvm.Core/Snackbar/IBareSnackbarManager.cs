using System;
using System.Collections.Generic;
using System.Text;

namespace BareMvvm.Core.Snackbar
{
    public interface IBareSnackbarManager
    {
        void Show(string message, int msToBeVisible = 10000);

        void Show(string message, string buttonText, Action callback, int msToBeVisible = 10000);
    }
}
