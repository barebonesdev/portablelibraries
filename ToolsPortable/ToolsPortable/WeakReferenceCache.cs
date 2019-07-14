using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class WeakReferenceCache<K, V> : IDictionary<K, V> where V : class
    {
        private Dictionary<K, WeakReference> _cache = new Dictionary<K, WeakReference>();

        public V this[K key]
        {
            get
            {
                return (V)_cache[key].Target;
            }

            set
            {
                if (value == null)
                {
                    Remove(key);
                    return;
                }

                WeakReference reference;

                if (!_cache.TryGetValue(key, out reference))
                    _cache[key] = new WeakReference(value);

                else
                    reference.Target = value;
            }
        }

        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                return _cache.Keys;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return _cache.Values.Select(i => i.Target).OfType<V>().Where(i => i != null).ToArray();
            }
        }

        public void Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(K key, V value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _cache.Add(key, new WeakReference(value));
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            WeakReference reference;

            if (_cache.TryGetValue(item.Key, out reference))
                return reference.Target == item.Value;

            return false;
        }

        public bool ContainsKey(K key)
        {
            WeakReference reference;

            if (_cache.TryGetValue(key, out reference))
                return reference.IsAlive;

            return false;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _cache.ToDictionary(k => k.Key, v => v.Value.Target as V).Where(i => i.Value != null).GetEnumerator();
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            if (Contains(item))
                return Remove(item.Key);

            return false;
        }

        public bool Remove(K key)
        {
            return _cache.Remove(key);
        }

        public bool TryGetValue(K key, out V value)
        {
            WeakReference reference;

            if (_cache.TryGetValue(key, out reference))
            {
                value = reference.Target as V;

                if (value == null)
                    return false;

                return true;
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
