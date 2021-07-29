using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class PortableMessageDialog
    {
        public string Content { get; private set; }
        public string Title { get; private set; }
        public string PositiveText { get; private set; }
        public string NegativeText { get; private set; }

        public PortableMessageDialog(string content)
        {
            Content = content;
        }

        public PortableMessageDialog(string content, string title)
        {
            Content = content;
            Title = title;
        }

        public PortableMessageDialog(string content, string title, string positiveText, string negativeText)
        {
            Content = content;
            Title = title;
            PositiveText = positiveText;
            NegativeText = negativeText;
        }

        public Task<bool> ShowForResultAsync()
        {
            if (Extension != null)
                return Extension(this);

            throw new NotImplementedException();
        }

        public Task ShowAsync()
        {
            if (Extension != null)
                return Extension(this);

            return Task.FromResult(true);
        }

        public async void Show()
        {
            try
            {
                await ShowAsync();
            }
            catch { }
        }

        public static Func<PortableMessageDialog, Task<bool>> Extension;
    }
}
