using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ToolsPortable;

namespace ToolsPortableMost
{
    [CollectionDataContract]
    public class MyObservableList<T> : ObservableCollection<T>, IMyList<T>
    {
        public IFilter<T> Filter { get; set; }

        public InsertAdapter<T> InsertAdapter { get; set; }

        public IComparer<T> Comparer { get; set; }

        public MyObservableList()
        {

        }

        public MyObservableList(IEnumerable<T> list)
        {
            AddRange(list);
        }

        public void AddRange(IEnumerable<T> items)
        {
            IEnumerator<T> i = items.GetEnumerator();
            while (i.MoveNext())
                Add(i.Current);
        }

        private object _lock = new object();

        [OnDeserialized]
        public void onDeserialized(StreamingContext context)
        {
            _lock = new object();
            upperItems = new List<UpperListItem>();
        }

        public int compareItems(T item, T other)
        {
            if (Comparer == null)
                return ((IComparable)item).CompareTo(other);
            else
                return Comparer.Compare(item, other);
        }

        /// <summary>
        /// Will use InsertAdapter and Comparer and IFilter if they are set
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int InsertSorted(T item)
        {
            lock (_lock)
            {
                //if using an insert adapter, generate the new item
                if (InsertAdapter != null)
                    item = InsertAdapter.CreateItem(item);

                //if it's not comparable, then just add it natural order
                if (!(item is IComparable) && Comparer == null)
                {
                    InsertItem(Count, item);
                    return Count - 1;
                }

                if (Count == 0)
                {
                    InsertItem(Count, item);
                    return 0;
                }

                int left = 0;
                int right = Count - 1;
                int middle = right / 2;

                while (left < right)
                {
                    int compare = compareItems(item, this[middle]);

                    if (compare > 0) //this item is later
                    {
                        left = middle + 1;
                        middle = (right + left) / 2;
                    }

                    else if (compare < 0) //this item is sooner
                    {
                        right = middle - 1;
                        middle = (right + left) / 2;
                    }

                    else //equal
                    {
                        left = middle + 1;
                        right = left;
                        break;
                    }
                }

                while (left < Count && compareItems(item, this[left]) >= 0)
                    left++;

                InsertItem(left, item);
                return left;
            }
        }

        /// <summary>
        /// Will use insert adapter and comparer and IFilter if they are set. Observes the list for changes.
        /// </summary>
        /// <param name="items"></param>
        public void InsertSorted(System.Collections.IEnumerable items)
        {
            if (items is INotifyCollectionChanged)
                ((INotifyCollectionChanged)items).CollectionChanged += items_CollectionChanged;

            IEnumerator i = items.GetEnumerator();
            while (i.MoveNext())
                InsertSorted((T)i.Current);
        }

        void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        InsertSorted((T)e.NewItems[i]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                        Remove((T)e.OldItems[i]);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                        Remove((T)e.OldItems[i]);
                    for (int i = 0; i < e.NewItems.Count; i++)
                        InsertSorted((T)e.NewItems[i]);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new Exception("The list was dramatically reset.");
            }
        }

        /// <summary>
        /// Generates a sublist from the current list, using the given filter. Sublist continues to update with main list.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public MyObservableList<T> Sublist(IFilter<T> filter, SublistSortOption sortOption = SublistSortOption.UseComparer)
        {
            switch (sortOption)
            { 
                case SublistSortOption.UseComparer:
                    MyObservableList<T> list = new MyObservableList<T>()
                    {
                        Filter = filter,
                        Comparer = Comparer,
                        InsertAdapter = InsertAdapter
                    };

                    list.InsertSorted(this);

                    return list;


                case SublistSortOption.SameAsParentList:
                    return new MyObservableSublistSortSameAsParent<T>(this, filter, InsertAdapter);

                default:
                    throw new NotImplementedException();
            }
        }

        public MyObservableList<T> Sublist(Func<T, bool> filter, SublistSortOption sortOption = SublistSortOption.UseComparer)
        {
            return Sublist(new FuncFilter(filter), sortOption);
        }

        protected override void ClearItems()
        {
            lock (_lock)
            {
                base.ClearItems();
            }
        }

        protected bool ShouldInsert(T item)
        {
            return Filter == null || Filter.ShouldInsert(item);
        }

        protected new bool InsertItem(int index, T item)
        {
            lock (_lock)
            {
                if (this.ShouldInsert(item))
                {
                    base.InsertItem(index, item);
                    return true;
                }

                return false;
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (_lock)
            {
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, T item)
        {
            lock (_lock)
            {
                base.SetItem(index, item);
            }
        }

        private List<UpperListItem> upperItems = new List<UpperListItem>();
        public void InsertSorted(IEnumerable items, string propertyNameOfListInsideEachItem)
        {
            lock (_lock)
            {
                //if not already watching the list
                if (!upperItems.Contains(new UpperListItem() { Items = items }))
                    if (items is INotifyCollectionChanged)
                        ((INotifyCollectionChanged)items).CollectionChanged += UpperList_CollectionChanged;

                upperItems.Add(new UpperListItem() { Items = items, propertyName = propertyNameOfListInsideEachItem });


                IEnumerator i = items.GetEnumerator();
                while (i.MoveNext())
                {
                    addItemsFromProperty(i.Current, propertyNameOfListInsideEachItem);
                }
            }
        }

        private void removeItemsFromProperty(object item, string property)
        {
            PropertyInfo info = item.GetType().GetProperty(property);
            if (info == null)
                return;

            object obj = info.GetValue(item, null);

            if (obj != null && obj is IEnumerable)
            {
                //stop listening to the list
                if (obj is INotifyCollectionChanged)
                    ((INotifyCollectionChanged)obj).CollectionChanged -= items_CollectionChanged;

                //remove all the items that were in that list
                IEnumerator i = ((IEnumerable)obj).GetEnumerator();
                while (i.MoveNext())
                    if (i.Current is T)
                        Remove((T)i.Current);
            }
        }

        private void addItemsFromProperty(object item, string property)
        {
            PropertyInfo info = item.GetType().GetProperty(property);
            if (info == null)
                return;

            object obj = info.GetValue(item, null);

            if (obj != null && obj is IEnumerable)
                InsertSorted((IEnumerable)obj);
        }

        void UpperList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            string property = null;
            for (int i = 0; i < upperItems.Count; i++)
                if (upperItems[i].Items == sender)
                {
                    property = upperItems[i].propertyName;
                    break;
                }

            if (property == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    for (int i = 0; i < e.NewItems.Count; i++)
                        addItemsFromProperty(e.NewItems[i], property);

                    break;

                case NotifyCollectionChangedAction.Remove:

                    for (int i = 0; i < e.OldItems.Count; i++)
                        removeItemsFromProperty(e.OldItems[i], property);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                        removeItemsFromProperty(e.OldItems[i], property);
                    for (int i = 0; i < e.NewItems.Count; i++)
                        addItemsFromProperty(e.NewItems[i], property);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new Exception("The upper list was dramatically reset");
            }
        }

        private class FuncFilter : IFilter<T>
        {
            private Func<T, bool> _filter;

            public FuncFilter(Func<T, bool> filter)
            {
                _filter = filter;
            }

            public bool ShouldInsert(T itemToBeInserted)
            {
                return _filter.Invoke(itemToBeInserted);
            }
        }

        public class UpperListItem : IEquatable<UpperListItem>
        {
            public IEnumerable Items;
            public string propertyName;

            public bool Equals(UpperListItem other)
            {
                return Items == other.Items;
            }
        }
    }
}
