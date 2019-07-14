using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ToolsPortable.Indexing
{
    public class IndexedItemResult<T>
    {
        public T Item { get; private set; }

        public double Hits { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="countOfItemsBefore">Used so that we can preserve order while still using a dictionary (first added item will start with hits of 1, second added item will start with hits of 0, third with -1, etc...)</param>
        public IndexedItemResult(T item, int countOfItemsBefore)
        {
            Item = item;

            // 0/1 = 0    = 0
            // 1/2 = 0.5  = 0.25
            // 1/3 = 0.66 = 0.33
            // 1/4 = 0.75 = 0.375
            Hits = 1 - (countOfItemsBefore / (double)(countOfItemsBefore + 1)) * 0.5;
        }

        public void IncrementHits()
        {
            Hits++;
        }

        public override int GetHashCode()
        {
            // Always use the object pointer hash code, since we match based on reference, not Equals
            return RuntimeHelpers.GetHashCode(Item);
        }
    }
}
