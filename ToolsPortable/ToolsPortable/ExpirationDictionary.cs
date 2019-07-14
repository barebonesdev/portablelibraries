using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    [DataContract]
    public class ExpirationDictionary<K, V> : IDictionary<K, V>
    {
        [OnDeserialized]
        public void _onDeserialized(StreamingContext c)
        {
            _dictionary = new Dictionary<K, LinkedListNode<KeyValuePair<K, V>>>(_recentList.Count);

            LinkedListNode<KeyValuePair<K, V>> node = _recentList.First;

            while (node != null)
            {
                _dictionary.Add(node.Value.Key, node);

                node = node.Next;
            }
        }

        [DataMember]
        public LinkedList<KeyValuePair<K, V>> _recentList = new LinkedList<KeyValuePair<K, V>>();

        /// <summary>
        /// Do NOT use set. Must be public for data contract serialization.
        /// </summary>
        [DataMember]
        public int CountLimit { get; set; }

        private Dictionary<K, LinkedListNode<KeyValuePair<K, V>>> _dictionary = new Dictionary<K, LinkedListNode<KeyValuePair<K, V>>>();

        public ExpirationDictionary(int countLimit)
        {
            CountLimit = countLimit;
        }

        /// <summary>
        /// Throws exception if key is already in dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(K key, V value)
        {
            //if we're at our capacity
            if (Count >= CountLimit)
            {
                //remove the last item's key
                _dictionary.Remove(_recentList.Last.Value.Key);

                //and remove that from the recents list
                _recentList.RemoveLast();
            }

            LinkedListNode<KeyValuePair<K, V>> node = new LinkedListNode<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value));

            //add to the dictionary
            _dictionary.Add(key, node);

            //add to the recent list
            _recentList.AddFirst(node);
        }

        /// <summary>
        /// Does not move the key to top.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(K key)
        {
            return _dictionary.ContainsKey(key);
        }

        public ICollection<K> Keys
        {
            get { return _dictionary.Keys; }
        }

        public bool Remove(K key)
        {
            LinkedListNode<KeyValuePair<K, V>> node;
            if (_dictionary.TryGetValue(key, out node))
            {
                //remove the key
                _dictionary.Remove(key);
                
                //remove from recents
                _recentList.Remove(node);

                return true;
            }

            return false;
        }

        public bool TryGetValue(K key, out V value)
        {
            LinkedListNode<KeyValuePair<K, V>> temp;
            if (_dictionary.TryGetValue(key, out temp))
            {
                //move to front
                _recentList.Remove(temp);
                _recentList.AddFirst(temp);

                value = temp.Value.Value;
                return true;
            }

            value = default(V);
            return false;
        }

        /// <summary>
        /// Has to create a new list.
        /// </summary>
        public ICollection<V> Values
        {
            get
            {
                List<V> list = new List<V>(Count);

                foreach (KeyValuePair<K, V> pair in _recentList)
                    list.Add(pair.Value);

                return list;
            }
        }

        public V this[K key]
        {
            get
            {
                //get the node
                LinkedListNode<KeyValuePair<K, V>> node = _dictionary[key];

                //move to front of list
                _recentList.Remove(node);
                _recentList.AddFirst(node);

                //return node's value
                return node.Value.Value;
            }
            set
            {
                //if we have the key already
                LinkedListNode<KeyValuePair<K, V>> node;
                if (_dictionary.TryGetValue(key, out node))
                {
                    //move node to front
                    _recentList.Remove(node);
                    _recentList.AddFirst(node);

                    //change value of the node
                    node.Value = new KeyValuePair<K, V>(key, value);
                }

                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _recentList.Clear();
        }

        /// <summary>
        /// Does not change the recent quality of the item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<K, V> item)
        {
            LinkedListNode<KeyValuePair<K, V>> node;
            if (_dictionary.TryGetValue(item.Key, out node))
                if (node.Value.Value.Equals(item.Value))
                    return true;

            return false;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            LinkedListNode<KeyValuePair<K, V>> node;
            if (_dictionary.TryGetValue(item.Key, out node))
            {
                if (node.Value.Value.Equals(item.Value))
                {
                    _recentList.Remove(node);

                    _dictionary.Remove(item.Key);

                    return true;
                }
            }

            return false;
        }

        private class ExpirationIterator : IEnumerator<KeyValuePair<K, V>>
        {
            private IEnumerator<KeyValuePair<K, LinkedListNode<KeyValuePair<K, V>>>> _iterator;

            public ExpirationIterator(IEnumerator<KeyValuePair<K, LinkedListNode<KeyValuePair<K, V>>>> iterator)
            {
                _iterator = iterator;
            }

            public KeyValuePair<K, V> Current
            {
                get { return _iterator.Current.Value.Value; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return _iterator.MoveNext();
            }

            public void Reset()
            {
                _iterator.Reset();
            }

            public void Dispose()
            {
                _iterator.Dispose();
            }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new ExpirationIterator((_dictionary as IDictionary<K, LinkedListNode<KeyValuePair<K, V>>>).GetEnumerator());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
