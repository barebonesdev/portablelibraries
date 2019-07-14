using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class WeakReferenceList<T> : ICollection<T> where T : class
    {
        private List<WeakReference<T>> _list = new List<WeakReference<T>>();

        /// <summary>
        /// Note that this count may not be accurate, it does not check that references are still active.
        /// </summary>
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            _list.Add(new WeakReference<T>(item));
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            foreach (var i in this)
            {
                if (object.Equals(i, item))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException("You should not use CopyTo with a WeakReferenceList, since the count you previously obtained could have changed by the time you called this method, which means you previously thought you would be getting 6 items but now are only getting 4.");
        }

        private bool _hasStale;
        public IEnumerator<T> GetEnumerator()
        {
            if (_hasStale)
            {
                // A previous time we enumerated, there were stale, so we'll clean up first
                CleanUpStaleReferences();
            }

            foreach (var refItem in _list)
            {
                T item;
                if (refItem.TryGetTarget(out item))
                {
                    yield return item;
                }
                else
                {
                    // We'll flag that there's stale, so that next time, we'll clean up
                    _hasStale = true;
                }
            }
        }

        public bool Remove(T item)
        {
            int index = 0;
            foreach (var refItem in _list)
            {
                T i;
                if (refItem.TryGetTarget(out i) && object.Equals(i, item))
                {
                    _list.RemoveAt(index);
                    return true;
                }

                index++;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CleanUpStaleReferences()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                T item;
                if (!_list[i].TryGetTarget(out item))
                {
                    _list.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
