using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    public interface IFilter<T>
    {
        bool ShouldInsert(T itemToBeInserted);
    }

    public interface InsertAdapter<T>
    {
        T CreateItem(T incoming);
    }

    public interface IMyList<T> : IList<T>
    {
        /// <summary>
        /// Does not get serialized. Filters items when added.
        /// </summary>
        IFilter<T> Filter { get; set; }

        /// <summary>
        /// Does not get serialized.
        /// </summary>
        InsertAdapter<T> InsertAdapter { get; set; }

        /// <summary>
        /// Does not get serialized. Only used for InsertSorted of lists to watch.
        /// </summary>
        IComparer<T> Comparer { get; set; }

        /// <summary>
        /// Inserts on the order of O(log(n)) using binary search, inserts at the correct location and returns the index it inserted at
        /// </summary>
        /// <param name="item">Must be of type T and implement IComparable"/></param>
        /// <returns></returns>
        int InsertSorted(T item);

        /// <summary>
        /// Inserts all the items, and listens to the list
        /// </summary>
        /// <param name="items"></param>
        void InsertSorted(IEnumerable items);

        /// <summary>
        /// Observes a main list of items, and adds the items inside each main item and observes them too
        /// </summary>
        /// <param name="items">The main collection of items, like a MyList(Class)</param>
        /// <param name="propertyNameOfListInsideEachItem">The property name of an item inside a main list item, like Homeworks, which is another list.</param>
        void InsertSorted(IEnumerable items, string propertyNameOfListInsideEachItem);
    }
}
