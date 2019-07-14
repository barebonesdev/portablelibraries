using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class MySublistsToFlatHeaderedList<TTopItem, TSubItem> : MyObservableList<object>
    {
        private Func<TTopItem, MyObservableList<TSubItem>> _selectChildrenFunc;
        private object _topHeader;
        public MySublistsToFlatHeaderedList(MyObservableList<TTopItem> source, Func<TTopItem, MyObservableList<TSubItem>> selectChildrenFunc, object topHeader)
        {
            if (topHeader != null)
            {
                _topHeader = topHeader;
                Add(topHeader);
            }
            
            _selectChildrenFunc = selectChildrenFunc;
            source.CollectionChanged += Source_CollectionChanged;

            AddTopLevelItems(0, source);
        }

        private void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    AddTopLevelItems(e.NewStartingIndex, e.NewItems.OfType<TTopItem>());
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    RemoveTopLevelItems(e.OldItems.OfType<TTopItem>());
                    AddTopLevelItems(e.NewStartingIndex, e.NewItems.OfType<TTopItem>());
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    RemoveTopLevelItems(e.OldItems.OfType<TTopItem>());
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    if (_topHeader == null)
                    {
                        base.Clear();
                    }
                    else
                    {
                        for (int i = Count - 1; i >= 1; i--)
                        {
                            base.RemoveAt(i);
                        }
                    }
                    AddTopLevelItems(0, (sender as MyObservableList<TTopItem>));
                    break;
            }
        }

        private void AddTopLevelItems(int originalIndex, IEnumerable<TTopItem> topItems)
        {
            int indexToInsertAt = GetAdaptedTopIndex(originalIndex);

            foreach (var topItem in topItems)
            {
                base.Insert(indexToInsertAt, topItem);
                indexToInsertAt++;

                var subItems = _selectChildrenFunc(topItem);
                subItems.CollectionChanged += SubItems_CollectionChanged;
                foreach (var subItem in subItems)
                {
                    base.Insert(indexToInsertAt, subItem);
                    indexToInsertAt++;
                }
            }
        }

        private void SubItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var subItemsList = (sender as MyObservableList<TSubItem>);

            int startOfSubItems = FindStartOfSubItems(subItemsList);
            if (startOfSubItems == -1)
            {
                // Parent wasn't found, de-register event
                // Although this theoretically should never get hit
                subItemsList.CollectionChanged -= SubItems_CollectionChanged;
                return;
            }

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        base.Insert(startOfSubItems + e.NewStartingIndex + i, e.NewItems[i]);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        base.RemoveAt(startOfSubItems + e.OldStartingIndex);
                    }
                    break;

                // Good chance the Move code has a bug... haven't actually tested it, just coded it by theory
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    if (e.NewStartingIndex > e.OldStartingIndex)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            base.Move(startOfSubItems + e.OldStartingIndex, startOfSubItems + e.NewStartingIndex + i);
                        }
                    }
                    else if (e.NewStartingIndex < e.OldStartingIndex)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            base.Move(startOfSubItems + e.OldStartingIndex + i, startOfSubItems + e.NewStartingIndex);
                        }
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        this[startOfSubItems + e.NewStartingIndex + i] = e.NewItems[i];
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    while (startOfSubItems < Count && this[startOfSubItems] is TSubItem)
                    {
                        base.RemoveAt(startOfSubItems);
                    }
                    for (int i = 0; i < subItemsList.Count; i++)
                    {
                        base.Insert(startOfSubItems + i, subItemsList[i]);
                    }
                    break;
            }
        }

        private int FindStartOfSubItems(MyObservableList<TSubItem> subItemsList)
        {
            for (int i = GetStartIndex(); i < Count; i++)
            {
                var item = this[i];

                // If the current item is the parent of this sublist
                if (item is TTopItem && _selectChildrenFunc((TTopItem)item) == subItemsList)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        private int GetAdaptedTopIndex(int originalTopIndex)
        {
            int countOfTopItemsSeen = 0;
            for (int i = GetStartIndex(); i < Count; i++)
            {
                if (this[i] is TTopItem)
                {
                    countOfTopItemsSeen++;
                    if (countOfTopItemsSeen > originalTopIndex)
                    {
                        return i;
                    }
                }
            }

            return Count;
        }

        private void RemoveTopLevelItems(IEnumerable<TTopItem> topItems)
        {
            List<TTopItem> toRemove = topItems.ToList();

            // Stop listening
            foreach (var removed in toRemove)
            {
                _selectChildrenFunc(removed).CollectionChanged -= SubItems_CollectionChanged;
            }

            for (int i = GetStartIndex(); i < Count; i++)
            {
                var item = this[i];
                if (item is TTopItem && toRemove.Contains((TTopItem)item))
                {
                    // Remove the header
                    base.RemoveAt(i);

                    // And remove all sub item children
                    while (i < Count && this[i] is TSubItem)
                    {
                        base.RemoveAt(i);
                    }

                    // If that was the last to remove, we're done
                    if (toRemove.Count == 0)
                        return;

                    // And then remove this from the toRemove list
                    toRemove.Remove((TTopItem)item);

                    // Decrement i so that we don't skip an item, since we removed
                    i--;
                }
            }
        }

        private int GetStartIndex()
        {
            return _topHeader == null ? 0 : 1;
        }
    }
}
