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
    public interface IMyObservableReadOnlyList<TItem> : IReadOnlyList<TItem>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    // Seems like I have to implement IList for the XAML list controls to data-bind to the collection
    public abstract class BaseMyObservableReadOnlyList<TItem> : IList<TItem>, INotifyCollectionChanged, IMyObservableReadOnlyList<TItem>, IList
    {
        public abstract TItem this[int index]
        {
            get; set;
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        TItem IList<TItem>.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public abstract int Count { get; }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return null;
            }
        }

        public abstract event NotifyCollectionChangedEventHandler CollectionChanged;
        public abstract event PropertyChangedEventHandler PropertyChanged;

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Add(TItem item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(TItem item)
        {
            return this.AsEnumerable().Contains(item);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public abstract IEnumerator<TItem> GetEnumerator();

        public int IndexOf(object value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (object.Equals(this[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(TItem item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (object.Equals(this[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TItem item)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TItem item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
