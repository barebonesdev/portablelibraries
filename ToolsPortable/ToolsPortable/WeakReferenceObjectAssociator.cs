using System;
using System.Collections.Generic;
using System.Text;

namespace ToolsPortable
{
    /// <summary>
    /// Allows you to associate objects with other objects, where the key object is weak referenced. Useful when associating objects with views and you don't want the views to be strongly referenced and want the associated object to persist only as long as the views are referenced. Holds a strong reference to the values (since otherwise those would be disposed immediately). NOT thread safe.
    /// </summary>
    public class WeakReferenceObjectAssociator<K, V> where K : class
    {
        private List<WeakReference<K>> _keys = new List<WeakReference<K>>();
        private List<V> _values = new List<V>();

        public V this[K key]
        {
            get
            {
                if (TryGetValue(key, out V value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                if (TryGetIndex(key, out int index))
                {
                    _values[index] = value;
                }
                else
                {
                    _keys.Add(new WeakReference<K>(key));
                    _values.Add(value);
                }
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            if (TryGetIndex(key, out int index))
            {
                value = _values[index];
                return true;
            }

            value = default(V);
            return false;
        }

        public bool TryGetIndex(K key, out int index)
        {
            int i = 0;
            foreach (var k in GetKeys())
            {
                if (object.ReferenceEquals(key, k))
                {
                    index = i;
                    return true;
                }

                i++;
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// Will remove all disposed values too, so the enumerator returned here will match up to the values list.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<K> GetKeys()
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i].TryGetTarget(out K k))
                {
                    yield return k;
                }
                else
                {
                    _keys.RemoveAt(i);
                    _values.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
