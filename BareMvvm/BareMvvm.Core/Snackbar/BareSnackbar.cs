using BareMvvm.Core.App;
using System;
using System.Collections.Generic;
using System.Text;

namespace BareMvvm.Core.Snackbar
{
    public static class BareSnackbar
    {
        public static void Show(string message, int msToBeVisible = 10000)
        {
            GetManager()?.Show(message, msToBeVisible);
        }

        public static void Show(string message, string buttonText, Action callback, int msToBeVisible = 10000)
        {
            GetManager()?.Show(message, buttonText, callback, msToBeVisible);
        }

        private static IBareSnackbarManager GetManager()
        {
            return PortableApp.Current?.GetCurrentWindow()?.NativeAppWindow?.SnackbarManager;
        }
    }
}
