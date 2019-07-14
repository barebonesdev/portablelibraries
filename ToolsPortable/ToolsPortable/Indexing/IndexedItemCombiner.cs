using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable.Indexing
{
    internal class IndexedItemCombiner<T>
    {
        private Dictionary<int, IndexedItemResult<T>> _results = new Dictionary<int, IndexedItemResult<T>>();

        public int Count
        {
            get { return _results.Count; }
        }

        public bool HasReachedMaxResults(int maxItems)
        {
            return Count >= maxItems;
        }



        /// <summary>
        /// Adds the finder. If the SearchItem is already in the list, then it promotes it. Has no duplicate SearchItems.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            IndexedItemResult<T> result;

            int hashCode = GetReferenceHashCode(item);

            if (_results.TryGetValue(hashCode, out result))
            {
                result.IncrementHits();
                return;
            }

            // We track the num of items before, so that items added first get extra weight, preserving order
            result = new IndexedItemResult<T>(item, _results.Count);

            _results.Add(hashCode, result);
        }

        public T[] GetSortedItems()
        {
            T[] answer = new T[this.Count];
            bool[] alreadyIncluded = new bool[this.Count];

            for (int i = 0; i < this.Count; i++)
            {
                double currMaxHits = double.MinValue;
                T currMaxItem = default(T);
                int currMaxIndex = 0;

                int x = -1;
                foreach (var pair in _results)
                {
                    x++;

                    if (alreadyIncluded[x])
                        continue;

                    if (pair.Value.Hits > currMaxHits)
                    {
                        currMaxHits = pair.Value.Hits;
                        currMaxItem = pair.Value.Item;
                        currMaxIndex = x;
                    }
                }

                answer[i] = currMaxItem;
                alreadyIncluded[currMaxIndex] = true;
            }

            return answer;
        }

        private static int GetReferenceHashCode(object item)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(item);
        }
    }
}
