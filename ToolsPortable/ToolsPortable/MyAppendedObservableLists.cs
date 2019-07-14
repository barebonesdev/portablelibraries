using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class ListWithItemSelector
    {
        public IEnumerable List { get; private set; }
        public Func<object, object> ItemSelector { get; private set; }

        private int? m_staticCount;
        public int GetCount()
        {
            if (m_staticCount != null)
            {
                return m_staticCount.Value;
            }

            return (List as IList).Count;
        }

        public object GetAt(int index)
        {
            if (List is IList)
            {
                return (List as IList)[index];
            }

            return List.OfType<object>().ElementAt(index);
        }

        public ListWithItemSelector(IEnumerable list, Func<object, object> itemSelector)
            : this(list)
        {
            ItemSelector = itemSelector;
        }

        public ListWithItemSelector(IEnumerable list)
        {
            List = list;

            if (list is IList)
            {
                // We're all good (we'll retrieve count directly)
            }
            else if (list is INotifyCollectionChanged)
            {
                // Invalid, we'll only track changes if it's a list
                throw new InvalidOperationException("Collection tracking is only enabled on list types, not enumerables/etc.");
            }
            else
            {
                // It's an enumerable without collection tracking - that means the items should never change. We statically do the count.
                m_staticCount = list.OfType<object>().Count();
            }
        }

        public object SelectItem(object original)
        {
            if (ItemSelector != null)
            {
                return ItemSelector(original);
            }

            return original;
        }

        public IEnumerable<T> GetEnumerable<T>()
        {
            foreach (var item in List)
            {
                yield return (T)SelectItem(item);
            }
        }
    }

    public class MyAppendedObservableLists<T> : INotifyCollectionChanged, IList<T>, IReadOnlyList<T>, IList
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private ListWithItemSelector[] _lists;

        public MyAppendedObservableLists(params IEnumerable[] lists) : this(lists.Select(i => new ListWithItemSelector(i)).ToArray())
        {
        }

        public MyAppendedObservableLists(params ListWithItemSelector[] lists)
        {
            if (lists == null)
            {
                throw new NullReferenceException(nameof(lists));
            }

            if (lists.Length <= 1)
            {
                throw new ArgumentException("lists must contain at least 2 lists");
            }

            _lists = lists.ToArray();

            foreach (var l in _lists)
            {
                if (l.List is INotifyCollectionChanged)
                {
                    (l.List as INotifyCollectionChanged).CollectionChanged += InnerList_CollectionChanged;
                }
            }
        }

        private void InnerList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int relativeStartIndex = 0;
            for (int i = 0; i < _lists.Length; i++)
            {
                if (sender == _lists[i].List)
                {
                    break;
                }
                else
                {
                    relativeStartIndex += _lists[i].GetCount();
                }
            }

            int relativeNewStartingIndex = e.NewStartingIndex + relativeStartIndex;
            int relativeOldStartingIndex = e.OldStartingIndex + relativeStartIndex;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;

                case NotifyCollectionChangedAction.Add:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        changedItems: e.NewItems,
                        startingIndex: relativeNewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Move:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Move,
                        e.NewItems,
                        relativeNewStartingIndex,
                        relativeOldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        changedItems: e.OldItems,
                        startingIndex: relativeOldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        newItems: e.NewItems,
                        oldItems: e.OldItems,
                        startingIndex: relativeNewStartingIndex));
                    break;
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var l in _lists)
                {
                    count += l.GetCount();
                }
                return count;
            }
        }

        public bool IsReadOnly => true;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        object IList.this[int index] { get { return this[index]; } set { throw ReadOnlyListException(); } }
        T IList<T>.this[int index] { get { return this[index]; } set { throw ReadOnlyListException(); } }

        public T this[int index]
        {
            get
            {
                foreach (var l in _lists)
                {
                    if (index < l.GetCount())
                    {
                        return (T)l.SelectItem(l.GetAt(index));
                    }

                    index -= l.GetCount();
                }

                throw new IndexOutOfRangeException("Index " + index + " wasn't found");
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new IEnumerableLinker<T>(_lists.Select(i => i.GetEnumerable<T>()).ToArray()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            int i = 0;
            foreach (var el in this)
            {
                if (object.Equals(el, item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw ReadOnlyListException();
        }

        public void RemoveAt(int index)
        {
            throw ReadOnlyListException();
        }

        public void Add(T item)
        {
            throw ReadOnlyListException();
        }

        public void Clear()
        {
            throw ReadOnlyListException();
        }

        public bool Contains(T item)
        {
            return this.Contains<T>(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw ReadOnlyListException();
        }

        public int Add(object value)
        {
            throw ReadOnlyListException();
        }

        public bool Contains(object value)
        {
            if (value is T)
            {
                return Contains((T)value);
            }

            return false;
        }

        public int IndexOf(object value)
        {
            if (value is T)
            {
                return IndexOf((T)value);
            }

            return -1;
        }

        public void Insert(int index, object value)
        {
            throw ReadOnlyListException();
        }

        public void Remove(object value)
        {
            throw ReadOnlyListException();
        }

        public void CopyTo(Array array, int index)
        {
            this.ToArray().CopyTo(array, index);
        }

        private static Exception ReadOnlyListException()
        {
            return new InvalidOperationException("Read only list");
        }
    }
}
