using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    [DataContract]
    public class MyHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        [DataMember]
        internal Dictionary<T, bool> _dictionary = new Dictionary<T, bool>();

        /// <summary>
        /// Returns true if it was added, false if it was already in there
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            _dictionary[item] = true;
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var pair in _dictionary)
                array[arrayIndex++] = pair.Key;
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return _dictionary.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
