using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    /// <summary>
    /// A list with InsertSorted capabilities. Does NOT lock.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class MyList<T> : List<T>, IMyList<T>
    {
        public IFilter<T> Filter { get; set; }

        public InsertAdapter<T> InsertAdapter { get; set; }

        public IComparer<T> Comparer { get; set; }

        public MyList() { }

        public MyList(int capacity) : base(capacity) { }

        public MyList(IEnumerable<T> list) : base(list) { }

        /// <summary>
        /// Inserts on the order of O(n - lastIndex) by using the starting index and iterating through
        /// </summary>
        /// <param name="item"></param>
        /// <param name="lastIndex"></param>
        /// <returns></returns>
        public int InsertSorted(T item, int lastIndex)
        {
            if (lastIndex < 0)
                lastIndex = 0;
            else if (lastIndex > Count)
                lastIndex = Count;

            //if it's not comparable, then just add it natural order
            if (!(item is IComparable) && Comparer == null)
            {
                Insert(lastIndex++, item);
                return lastIndex;
            }

            for (; lastIndex < Count; lastIndex++)
            {
                if (compareItems(item, this[lastIndex]) < 0)
                {
                    Insert(lastIndex, (T)item);
                    return lastIndex;
                }
            }

            Insert(lastIndex, (T)item);
            return lastIndex;
        }

        private int compareItems(T item, T other)
        {
            if (Comparer != null)
                return Comparer.Compare(item, other);

            return ((IComparable)item).CompareTo(other);
        }

        public int FindIndexForInsert(T item)
        {
            //if it's not comparable, then just add it natural order
            if (!(item is IComparable) && Comparer == null)
            {
                return Count;
            }

            if (Count == 0)
            {
                Add(item);
                return 0;
            }

            int left = 0;
            int right = Count - 1;
            int middle = right / 2;

            while (left < right)
            {
                //int compare = ((IComparable)item).CompareTo(this[middle]);
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
            
            return left;
        }

        /// <summary>
        /// Inserts on the order of O(log(n)) using binary search, inserts at the correct location and returns the index it inserted at
        /// </summary>
        /// <param name="item">Must be of type T and implement IComparable"/></param>
        /// <returns></returns>
        public int InsertSorted(T item)
        {
            int indexForInsert = FindIndexForInsert(item);

            Insert(indexForInsert, item);

            return indexForInsert;
        }

        /// <summary>
        /// Inserts all the items, but doesn't listen to the list as 
        /// </summary>
        /// <param name="items"></param>
        public void InsertSorted(IEnumerable items)
        {
            IEnumerator i = items.GetEnumerator();
            while (i.MoveNext())
                InsertSorted((T)i.Current);
        }


        public void InsertSorted(IEnumerable items, string propertyNameOfListInsideEachItem)
        {
            IEnumerator i = items.GetEnumerator();
            while (i.MoveNext())
            {
                PropertyInfo info = i.Current.GetType().GetRuntimeProperty(propertyNameOfListInsideEachItem);
                if (info != null)
                {
                    object obj = info.GetValue(i.Current, null);

                    if (obj != null && obj is IEnumerable)
                        InsertSorted((IEnumerable)obj);
                }
            }
        }
    }

    //[DataContract]
    //public class MyList<T> : IMyList<T>
    //{
    //    public IFilter<T> Filter { get; set; }

    //    public InsertAdapter<T> InsertAdapter { get; set; }

    //    public IComparer<T> Comparer { get; set; }

    //    [DataMember]
    //    public List<T> list;

    //    [OnDeserialized]
    //    public void onDeserialized(StreamingContext context)
    //    {
    //        _lock = new object();
    //    }

    //    private object _lock = new object();

    //    public MyList()
    //    {
    //        list = new List<T>();
    //    }

    //    public MyList(int capacity)
    //    {
    //        list = new List<T>(capacity);
    //    }

    //    public MyList(IEnumerable<T> list)
    //    {
    //        this.list = new List<T>(list);
    //    }

    //    public void AddRange(IEnumerable<T> items)
    //    {
    //        if (items == null)
    //            return;

    //        IEnumerator<T> i = items.GetEnumerator();
    //        while (i.MoveNext())
    //            Add(i.Current);
    //    }

    //    /// <summary>
    //    /// Inserts on the order of O(n - lastIndex) by using the starting index and iterating through
    //    /// </summary>
    //    /// <param name="item"></param>
    //    /// <param name="lastIndex"></param>
    //    /// <returns></returns>
    //    public int InsertSorted(T item, int lastIndex)
    //    {
    //        if (lastIndex < 0)
    //            lastIndex = 0;
    //        else if (lastIndex > Count)
    //            lastIndex = Count;

    //        //if it's not comparable, then just add it natural order
    //        if (!(item is IComparable) && Comparer == null)
    //        {
    //            Insert(lastIndex++, item);
    //            return lastIndex;
    //        }

    //        for (; lastIndex < Count; lastIndex++)
    //        {
    //            if (compareItems(item, this[lastIndex]) < 0)
    //            {
    //                Insert(lastIndex, (T)item);
    //                return lastIndex;
    //            }
    //        }

    //        Insert(lastIndex, (T)item);
    //        return lastIndex;
    //    }

    //    private int compareItems(T item, T other)
    //    {
    //        if (Comparer != null)
    //            return Comparer.Compare(item, other);

    //        return ((IComparable)item).CompareTo(other);
    //    }

    //    /// <summary>
    //    /// Inserts on the order of O(log(n)) using binary search, inserts at the correct location and returns the index it inserted at
    //    /// </summary>
    //    /// <param name="item">Must be of type T and implement IComparable"/></param>
    //    /// <returns></returns>
    //    public int InsertSorted(T item)
    //    {
    //        //if it's not comparable, then just add it natural order
    //        if (!(item is IComparable) && Comparer == null)
    //        {
    //            Add(item);
    //            return Count - 1;
    //        }

    //        if (Count == 0)
    //        {
    //            Add(item);
    //            return 0;
    //        }

    //        int left = 0;
    //        int right = Count - 1;
    //        int middle = right / 2;

    //        while (left < right)
    //        {
    //            //int compare = ((IComparable)item).CompareTo(this[middle]);
    //            int compare = compareItems(item, this[middle]);

    //            if (compare > 0) //this item is later
    //            {
    //                left = middle + 1;
    //                middle = (right + left) / 2;
    //            }

    //            else if (compare < 0) //this item is sooner
    //            {
    //                right = middle - 1;
    //                middle = (right + left) / 2;
    //            }

    //            else //equal
    //            {
    //                left = middle + 1;
    //                right = left;
    //                break;
    //            }
    //        }

    //        while (left < Count && compareItems(item, this[left]) >= 0)
    //            left++;

    //        Insert(left, item);
    //        return left;
    //    }

    //    /// <summary>
    //    /// Inserts all the items, but doesn't listen to the list as 
    //    /// </summary>
    //    /// <param name="items"></param>
    //    public void InsertSorted(IEnumerable items)
    //    {
    //        IEnumerator i = items.GetEnumerator();
    //        while (i.MoveNext())
    //            InsertSorted((T)i.Current);
    //    }

    //    public int IndexOf(T item)
    //    {
    //        lock (_lock)
    //        {
    //            return list.IndexOf(item);
    //        }
    //    }

    //    public void Insert(int index, T item)
    //    {
    //        lock (_lock)
    //        {
    //            if (Filter == null || Filter.ShouldInsert(item))
    //                list.Insert(index, item);
    //        }
    //    }

    //    public void RemoveAt(int index)
    //    {
    //        lock (_lock)
    //        {
    //            list.RemoveAt(index);
    //        }
    //    }

    //    public T this[int index]
    //    {
    //        get
    //        {
    //            lock (_lock)
    //            {
    //                return list[index];
    //            }
    //        }
    //        set
    //        {
    //            lock (_lock)
    //            {
    //                list[index] = value;
    //            }
    //        }
    //    }

    //    public void Add(T item)
    //    {
    //        lock (_lock)
    //        {
    //            if (Filter == null || Filter.ShouldInsert(item))
    //                list.Add(item);
    //        }
    //    }

    //    public void Clear()
    //    {
    //        lock (_lock)
    //        {
    //            list.Clear();
    //        }
    //    }

    //    public bool Contains(T item)
    //    {
    //        lock (_lock)
    //        {
    //            return list.Contains(item);
    //        }
    //    }

    //    public void CopyTo(T[] array, int arrayIndex)
    //    {
    //        lock (_lock)
    //        {
    //            list.CopyTo(array, arrayIndex);
    //        }
    //    }

    //    public int Count
    //    {
    //        get
    //        {
    //            lock (_lock)
    //            {
    //                return list.Count;
    //            }
    //        }
    //    }

    //    public bool IsReadOnly
    //    {
    //        get { return false; }
    //    }

    //    public bool Remove(T item)
    //    {
    //        lock (_lock)
    //        {
    //            return list.Remove(item);
    //        }
    //    }

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        lock (_lock)
    //        {
    //            List<T> copied = new List<T>(list);
    //            return copied.GetEnumerator();
    //        }
    //    }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        lock (_lock)
    //        {
    //            List<T> copied = new List<T>(list);
    //            return copied.GetEnumerator();
    //        }
    //    }


    //    public void InsertSorted(IEnumerable items, string propertyNameOfListInsideEachItem)
    //    {
    //        lock (_lock)
    //        {
    //            IEnumerator i = items.GetEnumerator();
    //            while (i.MoveNext())
    //            {
    //                PropertyInfo info = i.Current.GetType().GetProperty(propertyNameOfListInsideEachItem);
    //                if (info != null)
    //                {
    //                    object obj = info.GetValue(i.Current, null);

    //                    if (obj != null && obj is IEnumerable)
    //                        InsertSorted((IEnumerable)obj);
    //                }
    //            }
    //        }
    //    }
    //}
}
