using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;

namespace ToolsPortableMost
{
    public abstract class MyObservableSublist<T> : MyObservableList<T>
    {
        public MyObservableList<T> ParentList { get; private set; }
        public SublistSortOption SortOption { get; private set; }

        public MyObservableSublist(MyObservableList<T> parentList, SublistSortOption sortOption, IFilter<T> filter, IComparer<T> comparer, InsertAdapter<T> insertAdapter)
        {
            if (parentList == null)
                throw new ArgumentNullException("parentList cannot be null");

            base.Filter = filter;
            base.Comparer = comparer;
            base.InsertAdapter = insertAdapter;

            ParentList = parentList;
            SortOption = sortOption;

            Reset();

            parentList.CollectionChanged += parentList_CollectionChanged;
        }

        void parentList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    HandleAdd(e.NewStartingIndex, e.NewItems);
                    break;


                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    HandleRemove(e.OldStartingIndex, e.OldItems);
                    break;


                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    HandleRemove(e.OldStartingIndex, e.OldItems);
                    HandleAdd(e.NewStartingIndex, e.NewItems);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
            }
        }

        protected virtual void HandleRemove(int index, IList removedItems)
        {
            foreach (object item in removedItems)
                base.Remove((T)item);
        }

        protected abstract void HandleAdd(int index, IList newItems);

        protected virtual void Reset()
        {
            base.Clear();

            HandleAdd(0, ParentList);
        }




        
    }

    public class MyObservableSublistSortSameAsParent<T> : MyObservableSublist<T>
    {
        private List<bool> _indexTracking = new List<bool>();

        internal MyObservableSublistSortSameAsParent(MyObservableList<T> parentList, IFilter<T> filter, InsertAdapter<T> insertAdapter)
            : base(parentList, SublistSortOption.SameAsParentList, filter, null, insertAdapter)
        {

        }

        protected override void HandleRemove(int index, IList removedItems)
        {
            for (int x = 0; x < removedItems.Count; x++)
            {
                int parentIndex = index + x;

                // If included in sublist, remove it
                if (IsIncluded(parentIndex))
                    base.RemoveAt(GetAdaptedIndex(parentIndex));

                // Remove from index tracking
                _indexTracking.RemoveAt(parentIndex);
            }
        }

        protected override void Reset()
        {
            // reset the index tracking
            _indexTracking.Clear();

            base.Reset();
        }

        protected override void HandleAdd(int index, IList newItems)
        {
            for (int i = 0; i < newItems.Count; i++)
            {
                T item = (T)newItems[i];

                bool inserted = base.InsertItem(GetAdaptedIndex(index + i), item);

                _indexTracking.Insert(index + i, inserted);
            }
        }

        private bool IsIncluded(int parentIndex)
        {
            return _indexTracking[parentIndex];
        }

        private int GetAdaptedIndex(int parentIndex)
        {
            int adaptedIndex = 0;

            for (int i = 0; i < parentIndex; i++)
                if (_indexTracking[i])
                    adaptedIndex++;

            return adaptedIndex;
        }
    }
}
