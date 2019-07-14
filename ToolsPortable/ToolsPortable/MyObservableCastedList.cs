using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class MyObservableCastedList<TSource, TFinal> : BaseMyObservableReadOnlyList<TFinal> where TFinal : TSource
    {
        private IReadOnlyList<TSource> _original;

        public MyObservableCastedList(IReadOnlyList<TSource> original)
        {
            _original = original;

            if (original is INotifyCollectionChanged)
            {
                (original as INotifyCollectionChanged).CollectionChanged += MyObservableCastedList_CollectionChanged;
            }
        }

        private void MyObservableCastedList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    break;

                default:
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    break;
            }
        }

        public override int Count
        {
            get
            {
                return _original.Count;
            }
        }

        public override TFinal this[int index]
        {
            get
            {
                return (TFinal)_original[index];
            }
            set { throw new NotImplementedException(); }
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        public override event PropertyChangedEventHandler PropertyChanged;

        public override IEnumerator<TFinal> GetEnumerator()
        {
            return _original.OfType<TFinal>().GetEnumerator();
        }
    }
}
