using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    [CollectionDataContract(Namespace = "http://schemas.datacontract.org/2004/07/ToolsUniversal")]
    public class MyObservableList<T> : ObservableCollection<T>, IMyList<T>, IMyObservableReadOnlyList<T>
    {
        public IFilter<T> Filter { get; set; }

        public InsertAdapter<T> InsertAdapter { get; set; }

        public IComparer<T> Comparer { get; set; }

        public MyObservableList()
        {
            _itemsCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(items_CollectionChanged).Handler;
        }

        public MyObservableList(IEnumerable<T> list) : base()
        {
            AddRange(list);
        }

        public void AddRange(IEnumerable<T> items)
        {
            IEnumerator<T> i = items.GetEnumerator();
            while (i.MoveNext())
                Add(i.Current);
        }

        [OnDeserialized]
        public void onDeserialized(StreamingContext context)
        {
            upperItems = new List<UpperListItem>();
        }

        public int CompareItems(T item, T other)
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
                int compare = CompareItems(item, this[middle]);

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

            while (left < Count && CompareItems(item, this[left]) >= 0)
                left++;

            InsertItem(left, item);
            return left;
        }

        /// <summary>
        /// Use this if you've used InsertSorted to insert and watch a list, and now want to stop watching it
        /// </summary>
        /// <param name="collection"></param>
        public void StopWatchingList(INotifyCollectionChanged collection)
        {
            _listsWeAreObserving.Remove(collection);
            collection.CollectionChanged -= _itemsCollectionChangedHandler;
        }

        public void EndMakeThisACopyOf(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= CopiedItems_CollectionChanged;
        }

        public void MakeThisACopyOf(System.Collections.IEnumerable items)
        {
            if (items is INotifyCollectionChanged)
                (items as INotifyCollectionChanged).CollectionChanged += CopiedItems_CollectionChanged;

            this.Clear();

            this.AddRange(items.OfType<T>());


        }

        private void CopiedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        this.Insert(i + e.NewStartingIndex, (T)e.NewItems[i]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                        this.RemoveAt(i + e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        this[i + e.OldStartingIndex] = (T)e.NewItems[i];
                    break;

                case NotifyCollectionChangedAction.Move:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        this.Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.Clear();
                    this.AddRange((sender as IEnumerable).OfType<T>());
                    break;
            }
        }

        private NotifyCollectionChangedEventHandler _itemsCollectionChangedHandler;
        private List<INotifyCollectionChanged> _listsWeAreObserving = new List<INotifyCollectionChanged>();

        /// <summary>
        /// Will use insert adapter and comparer and IFilter if they are set. Observes the list for changes.
        /// </summary>
        /// <param name="items"></param>
        public void InsertSorted(System.Collections.IEnumerable items, bool trackChanges)
        {
            if (trackChanges && items is INotifyCollectionChanged)
            {
                (items as INotifyCollectionChanged).CollectionChanged += _itemsCollectionChangedHandler;
                _listsWeAreObserving.Add(items as INotifyCollectionChanged);
            }

            IEnumerator i = items.GetEnumerator();
            while (i.MoveNext())
            {
                if (i.Current is T)
                {
                    InsertSorted((T)i.Current);
                }
            }
        }

        /// <summary>
        /// Will use insert adapter and comparer and IFilter if they are set. Observes the list for changes.
        /// </summary>
        /// <param name="items"></param>
        public void InsertSorted(System.Collections.IEnumerable items)
        {
            InsertSorted(items, trackChanges: true);
        }

        public void StopObservingCollection(INotifyCollectionChanged collection)
        {
            _listsWeAreObserving.Remove(collection);
            collection.CollectionChanged -= _itemsCollectionChangedHandler;
        }

        void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var i in e.NewItems.OfType<T>())
                        InsertSorted(i);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var i in e.OldItems.OfType<T>())
                        Remove(i);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var i in e.OldItems.OfType<T>())
                        Remove(i);
                    foreach (var i in e.NewItems.OfType<T>())
                        InsertSorted(i);
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
        public MyObservableList<T> Sublist(IFilter<T> filter)
        {
            MyObservableList<T> list = new MyObservableList<T>()
            {
                Filter = filter,
                Comparer = Comparer,
                InsertAdapter = InsertAdapter
            };

            list.InsertSorted(this);

            return list;
        }

        /// <summary>
        /// Insertion sort (used because it should minimize number of moves) from https://www.cs.cmu.edu/~adamchik/15-121/lectures/Sorting%20Algorithms/sorting.html
        /// </summary>
        public void Sort()
        {
            for (int i = 0; i < this.Count; i++)
            {
                var minItem = this[i];
                var minItemIndex = i;

                for (int j = i + 1; j < this.Count; j++)
                {
                    var compItem = this[j];
                    
                    if (CompareItems(compItem, minItem) < 0)
                    {
                        minItem = compItem;
                        minItemIndex = j;
                    }
                }

                if (minItemIndex != i)
                    this.Move(minItemIndex, i);
            }
        }

        public class FilterUsingFunction : IFilter<T>
        {
            private Func<T, bool> _func;
            public FilterUsingFunction(Func<T, bool> func)
            {
                _func = func;
            }

            public bool ShouldInsert(T itemToBeInserted)
            {
                return _func.Invoke(itemToBeInserted);
            }
        }

        public MyObservableList<T> Sublist(Func<T, bool> filter)
        {
            return Sublist(new FilterUsingFunction(filter));
        }

        public IMyObservableReadOnlyList<TFinal> OfTypeObservable<TFinal>() where TFinal : T
        {
            return new MyObservableOfTypeList<T, TFinal>(this);
        }

        public IMyObservableReadOnlyList<TFinal> Cast<TFinal>() where TFinal : T
        {
            return new MyObservableCastedList<T, TFinal>(this);
        }

        /// <summary>
        /// Will continue to observe and react to changes
        /// </summary>
        /// <returns></returns>
        public MyObservableList<T> ToSortedList()
        {
            var list = new MyObservableList<T>();
            list.InsertSorted(this);
            return list;
        }

        protected override void InsertItem(int index, T item)
        {
            if (Filter == null || Filter.ShouldInsert(item))
                base.InsertItem(index, item);
        }

        private List<UpperListItem> upperItems = new List<UpperListItem>();
        [Obsolete("Shouldn't use this anymore, doesn't handle the drastic reset. Instead, implement yourself.")]
        public void InsertSorted(IEnumerable items, string propertyNameOfListInsideEachItem)
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

        private void removeItemsFromProperty(object item, string property)
        {
            PropertyInfo info = item.GetType().GetRuntimeProperty(property);
            if (info == null)
                return;

            object obj = info.GetValue(item, null);

            if (obj != null && obj is IEnumerable)
            {
                //stop listening to the list
                if (obj is INotifyCollectionChanged)
                {
                    _listsWeAreObserving.Remove(obj as INotifyCollectionChanged);
                    ((INotifyCollectionChanged)obj).CollectionChanged -= _itemsCollectionChangedHandler;
                }

                //remove all the items that were in that list
                IEnumerator i = ((IEnumerable)obj).GetEnumerator();
                while (i.MoveNext())
                    if (i.Current is T)
                        Remove((T)i.Current);
            }
        }

        private void addItemsFromProperty(object item, string property)
        {
            PropertyInfo info = item.GetType().GetRuntimeProperty(property);
            if (info == null)
                return;

            object obj = info.GetValue(item, null);

            if (obj is IEnumerable)
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
