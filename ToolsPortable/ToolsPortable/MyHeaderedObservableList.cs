using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class MyHeaderedObservableList<TItem, THeaderItem> : MyObservableList<object>
    {
        private Func<TItem, THeaderItem> _itemToHeaderFunc;
        private IMyObservableReadOnlyList<TItem> _source;
        private PropertyInfo _collapsedHeaderProperty;
        private Dictionary<THeaderItem, List<TItem>> _collapsedHeaders = new Dictionary<THeaderItem, List<TItem>>();
        
        public MyHeaderedObservableList(IMyObservableReadOnlyList<TItem> source, Func<TItem, THeaderItem> itemToHeaderFunc)
        {
            if (typeof(THeaderItem).GetTypeInfo().ImplementedInterfaces.Contains(typeof(INotifyPropertyChanged)))
            {
                var prop = typeof(THeaderItem).GetRuntimeProperty("Collapsed");
                if (prop.PropertyType == typeof(bool))
                {
                    _collapsedHeaderProperty = prop;
                }
            }

            _itemToHeaderFunc = itemToHeaderFunc;
            _source = source;
            source.CollectionChanged += Source_CollectionChanged;
            AddItems(0, source);
        }

        private void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var collapsedHeaders = _collapsedHeaders.Keys.ToArray();
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                collapsedHeaders = new THeaderItem[0];
            }

            if (collapsedHeaders.Length > 0)
            {
                // First we need to reset all to expanded so we can add properly
                foreach (var header in collapsedHeaders)
                {
                    _collapsedHeaderProperty.SetValue(header, false);
                }
            }

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    AddItems(e.NewStartingIndex, e.NewItems.OfType<TItem>());
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    RemoveItems(e.OldStartingIndex, e.OldItems.Count);
                    AddItems(e.NewStartingIndex, e.NewItems.OfType<TItem>());
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldStartingIndex, e.OldItems.Count);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldStartingIndex, e.OldItems.Count);
                    AddItems(e.NewStartingIndex, e.NewItems.OfType<TItem>());
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    if (_collapsedHeaderProperty != null)
                    {
                        foreach (var header in this.OfType<THeaderItem>())
                        {
                            (header as INotifyPropertyChanged).PropertyChanged -= Header_PropertyChanged;
                        }

                        _collapsedHeaders.Clear();
                    }
                    this.Clear();
                    AddItems(0, sender as IEnumerable<TItem>);
                    break;
            }

            if (collapsedHeaders.Length > 0)
            {
                // And then we re-collapse headers after modifications were made
                foreach (var header in collapsedHeaders)
                {
                    _collapsedHeaderProperty.SetValue(header, true);
                }
            }
        }

        private void RemoveItems(int originalIndex, int countOfItems)
        {
            int adaptedIndex = GetAdaptedIndex(originalIndex);

            for (int i = 0; i < countOfItems; i++)
            {
                // If the previous is a header, and there's no next item
                if (this.ElementAtOrDefault(adaptedIndex - 1) is THeaderItem headerItem && !(this.ElementAtOrDefault(adaptedIndex + 1) is TItem))
                {
                    // That means we're removing the last item from that group
                    // So we need to remove its header
                    this.RemoveAt(adaptedIndex - 1);
                    adaptedIndex--;

                    if (_collapsedHeaderProperty != null && headerItem is INotifyPropertyChanged headerPropertyChanged)
                    {
                        headerPropertyChanged.PropertyChanged -= Header_PropertyChanged;
                    }
                }

                this.RemoveAt(adaptedIndex);
            }
        }

        private void AddItems(int originalIndex, IEnumerable<TItem> items)
        {
            int adaptedIndex = GetAdaptedIndex(originalIndex);

            // If it's surrounded by items of its own type, we know that they're all under the current header
            if (adaptedIndex > 0 && this[adaptedIndex - 1] is TItem && this.ElementAtOrDefault(adaptedIndex) is TItem)
            {
                foreach (var item in items)
                {
                    base.Insert(adaptedIndex, item);
                    adaptedIndex++;
                }
                return;
            }

            // Otherwise we're at the border of a group (or there's no existing items)

            // This means [A1] the item potentially belongs in the previous group,
            // or [A2] it belongs in a new group between the two,
            // or [A3] it also could be the first item in this group
            // or [A4] this is the first item in the blank list

            // For the subsequent items, we know that they must either be in the same group as the first item,
            // or in an existing subsequent group,
            // or in a new group before an existing subsequent group

            HeaderForComparison currHeader;
            
            // [A4] If we're at the start, then there's no header (this should only get hit if there's zero items in the list right now)
            if (adaptedIndex == 0)
            {
                currHeader = HeaderForComparison.None;
            }

            // Otherwise there logically must be a header before this, which means [A1] we might be in the previous existing group,
            // or [A2] we're supposed to be in a new group between the two
            // or [A3] we're supposed to be in the current group
            else
            {
                // If we're at the end
                if (adaptedIndex == Count)
                {
                    // The prev item must be another item, get its group
                    currHeader = GetHeader(adaptedIndex - 1);
                }

                // Otherwise if there's a previous group,
                // we'll use that as the starting point, since we might be in it
                // this could still be cases [A1] [A2] [A3]
                else if (adaptedIndex - 2 >= 0)
                {
                    currHeader = GetHeader(adaptedIndex - 2);
                    adaptedIndex--;
                }

                // Otherwise, no previous group,
                // so we're potentially in [A2] a new group or [A3] the current group
                else
                {
                    currHeader = HeaderForComparison.None;
                    adaptedIndex--;
                }

                // In the two latter cases, we move the index back one so that it's on the header,
                // which means that when we insert, it'll insert before that header.
                // If we're supposed to be after the header, the loop will need to move the index.

                // For the end-of-the-list case, we're already in the correct position
            }

            // Adapted index will always be on a header (or the end of the list)
            
            foreach (var item in items)
            {
                THeaderItem thisItemHeader = CreateHeader(item);

                // If the header has changed
                if (!currHeader.Equals(thisItemHeader))
                {
                    bool foundExistingGroup = false;

                    // See if there is a subsequent existing group
                    if (adaptedIndex < Count)
                    {
                        var subsequentGroup = (THeaderItem)this[adaptedIndex];

                        // If we fit into that subsequent existing group
                        if (object.Equals(thisItemHeader, subsequentGroup))
                        {
                            adaptedIndex++;
                            foundExistingGroup = true;
                            currHeader = new HeaderForComparison(subsequentGroup);
                        }
                    }

                    // If we don't fit into that existing group, we must be a group between
                    if (!foundExistingGroup)
                    {
                        base.Insert(adaptedIndex, thisItemHeader);

                        if (_collapsedHeaderProperty != null && thisItemHeader is INotifyPropertyChanged headerPropertyChanged)
                        {
                            headerPropertyChanged.PropertyChanged += Header_PropertyChanged;
                        }

                        currHeader = new HeaderForComparison(thisItemHeader);
                        adaptedIndex++;
                    }
                }

                // Otherwise, the header is good

                // Add the item
                base.Insert(adaptedIndex, item);
                adaptedIndex++;
            }
        }

        private void Header_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Collapsed")
            {
                THeaderItem header = (THeaderItem)sender;

                // If collapsed
                if ((bool)_collapsedHeaderProperty.GetValue(header))
                {
                    if (!_collapsedHeaders.ContainsKey(header))
                    {
                        int headerIndex = IndexOf(header);
                        var itemsUnderHeader = this.Skip(headerIndex + 1).TakeWhile(i => i is TItem).OfType<TItem>().ToList();
                        _collapsedHeaders[header] = itemsUnderHeader;
                        for (int i = 0; i < itemsUnderHeader.Count; i++)
                        {
                            RemoveAt(headerIndex + 1);
                        }
                    }
                }

                // Otherwise if expanded
                else
                {
                    if (_collapsedHeaders.TryGetValue(header, out List<TItem> itemsUnderHeader))
                    {
                        int headerIndex = IndexOf(header);
                        for (int i = 0; i < itemsUnderHeader.Count; i++)
                        {
                            Insert(headerIndex + 1 + i, itemsUnderHeader[i]);
                        }
                        _collapsedHeaders.Remove(header);
                    }
                }
            }
        }

        private THeaderItem CreateHeader(TItem item)
        {
            return _itemToHeaderFunc(item);
        }

        private class HeaderForComparison : IEquatable<THeaderItem>
        {
            public static readonly HeaderForComparison None = new HeaderForComparison();

            public bool HasItem { get; private set; }
            public THeaderItem Item { get; private set; }

            public HeaderForComparison(THeaderItem item)
            {
                HasItem = true;
                Item = item;
            }

            private HeaderForComparison() { }

            public bool Equals(THeaderItem other)
            {
                if (!HasItem)
                {
                    return false;
                }

                return object.Equals(Item, other);
            }
        }

        private HeaderForComparison GetHeader(int adaptedIndex)
        {
            if (adaptedIndex == 0)
                return HeaderForComparison.None;

            for (int i = adaptedIndex - 1; i >= 0; i--)
            {
                if (this[i] is THeaderItem)
                    return new HeaderForComparison((THeaderItem)this[i]);
            }

            return HeaderForComparison.None;
        }

        private int GetAdaptedIndex(int originalIndex)
        {
            int stop = originalIndex;
            int i;
            for (i = 0; i <= stop; i++)
            {
                if (i >= Count)
                    return i;

                if (this[i] is THeaderItem header)
                {
                    if (_collapsedHeaders.TryGetValue(header, out List<TItem> collapsedItems))
                    {
                        stop += 1 - collapsedItems.Count;
                    }
                    else
                    {
                        stop++;
                    }
                }

                if (i == stop)
                    return i;
            }

            return i;
        }
    }

    public static class MyHeaderedObservableListExtensions
    {
        public static MyHeaderedObservableList<TItem, THeaderItem> ToHeaderedList<TItem, THeaderItem>(this IMyObservableReadOnlyList<TItem> source, Func<TItem, THeaderItem> itemToHeaderFunc)
        {
            return new MyHeaderedObservableList<TItem, THeaderItem>(source, itemToHeaderFunc);
        }
    }
}
