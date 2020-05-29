﻿using BareMvvm.Core.App;
using System;
using System.Collections.Generic;
using System.Text;

namespace BareMvvm.Core.Snackbar
{
    public class BareSnackbar
    {
        private const int DefaultMsToBeVisible = 5000;

        public string Message { get; private set; }
        public int Duration { get; private set; }
        public string ButtonText { get; private set; }
        public Action ButtonCallback { get; private set; }

        private BareSnackbar() { }

        public static BareSnackbar Make(string message, int msToBeVisible = DefaultMsToBeVisible)
        {
            return new BareSnackbar()
            {
                Message = message,
                Duration = msToBeVisible
            };
        }

        public static BareSnackbar Make(string message, string buttonText, Action callback, int msToBeVisible = DefaultMsToBeVisible)
        {
            return new BareSnackbar()
            {
                Message = message,
                Duration = msToBeVisible,
                ButtonText = buttonText,
                ButtonCallback = callback
            };
        }

        public void Show()
        {
            GetManager().Show(this);
        }

        private static BareSnackbarManager GetManager()
        {
            return PortableApp.Current?.GetCurrentWindow()?.NativeAppWindow?.SnackbarManager;
        }

        public void Close()
        {
            GetManager().Close(this);
        }
    }
}
