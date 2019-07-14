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
    public class MyObservableOfTypeList<TSource, TFinal> : BaseMyObservableReadOnlyList<TFinal> where TFinal : TSource
    {
        private MyObservableList<TSource> _filtered;

        public MyObservableOfTypeList(MyObservableList<TSource> source)
        {
            _filtered = source.Sublist(i => i is TFinal);
            _filtered.CollectionChanged += _filtered_CollectionChanged;
        }

        private void _filtered_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                return _filtered.Count;
            }
        }

        public override TFinal this[int index]
        {
            get
            {
                return (TFinal)_filtered[index];
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        public override event PropertyChangedEventHandler PropertyChanged;

        public override IEnumerator<TFinal> GetEnumerator()
        {
            return _filtered.OfType<TFinal>().GetEnumerator();
        }
    }
}
