using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public static class IListExtensions
    {
        public static bool RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate)
        {
            bool removedAny = false;

            for (int i = 0; i < list.Count; i++)
            {
                if (predicate.Invoke(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                    removedAny = true;
                }
            }

            return removedAny;
        }

        public static int FindIndex<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate.Invoke(list[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool MakeListLike<T>(this IList<T> list, IList<T> desired)
        {
            //if already equal, do nothing
            if (desired.SequenceEqual(list))
                return false;

            //remove any of the items that aren't there anymore
            for (int i = 0; i < list.Count; i++)
                if (!desired.Contains(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                }

            for (int i = 0; i < desired.Count; i++)
            {
                if (i >= list.Count)
                    list.Add(desired[i]);

                //there's a wrong item in its place
                else if (!object.Equals(list[i], desired[i]))
                {
                    //if it's already in the list somewhere, we remove it
                    list.Remove(desired[i]);

                    //no matter what we insert it into its desired spot
                    list.Insert(i, desired[i]);
                }

                //otherwise it's already in the right place!
            }

            return true;
        }
    }
}
