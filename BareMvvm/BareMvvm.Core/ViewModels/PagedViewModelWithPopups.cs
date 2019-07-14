using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace BareMvvm.Core.ViewModels
{
    public abstract class PagedViewModelWithPopups : PagedViewModel
    {
        public PagedViewModelWithPopups(BaseViewModel parent) : base(parent)
        {
            Popups.CollectionChanged += Popups_CollectionChanged;
        }

        public MyObservableList<BaseViewModel> Popups { get; private set; } = new MyObservableList<BaseViewModel>();

        public override void ShowPopup(BaseViewModel viewModel)
        {
            Popups.Add(viewModel);
        }

        protected override BaseViewModel GetChildContent()
        {
            if (Popups.Count > 0)
                return Popups.Last();

            return base.GetChildContent();
        }

        public override IEnumerable<BaseViewModel> GetChildren()
        {
            var list = new List<BaseViewModel>(base.GetChildren());
            list.AddRange(Popups);
            return list;
        }

        private BaseViewModel _prevLastPopup;
        private void Popups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Popups.LastOrDefault() != _prevLastPopup)
            {
                var prevLastPopup = _prevLastPopup;
                _prevLastPopup = Popups.LastOrDefault();

                var currPopup = Popups.LastOrDefault();
                
                // If we had a popup previously
                if (prevLastPopup != null)
                {
                    prevLastPopup.OnViewLostFocus();
                }
                // Or if we didn't have any other popups but just added a popup, the normal paged content was lost focus
                else if (currPopup != null && this.Content != null)
                {
                    this.Content.OnViewLostFocus();
                }

                if (currPopup != null)
                {
                    currPopup.OnViewFocused();
                }
                else if (this.Content != null)
                {
                    this.Content.OnViewFocused();
                }

                TriggerVisibleContentChanged();
            }
        }

        public override bool GoBack()
        {
            if (Popups.Count > 0)
            {
                Popups.RemoveAt(Popups.Count - 1);
                return true;
            }

            return base.GoBack();
        }

        public override bool RemoveViewModel(BaseViewModel model)
        {
            if (Popups.Remove(model))
            {
                return true;
            }

            return base.RemoveViewModel(model);
        }

        /// <summary>
        /// Tries to clear all popups under the user interaction lock. Won't throw exception.
        /// </summary>
        public async void TryClearPopupsViaUserInteraction()
        {
            try
            {
                await HandleUserInteractionAsync("ClearPopups", delegate
                {
                    Popups.Clear();
                });
            }
            catch (Exception ex)
            {
                ExceptionHelper.ReportHandledException(ex);
            }
        }
    }
}
