using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ToolsPortable.Indexing
{
    /// <summary>
    /// This is essentially a sorted list of words (with the item linked to each word).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class IndexedList<T>
    {
        public LinkedList<IndexedWord<T>> _list = new LinkedList<IndexedWord<T>>();

        /// <summary>
        /// Returns the list of the indexed items.
        /// </summary>
        public LinkedList<IndexedWord<T>> List
        {
            get { return _list; }
        }

        /// <summary>
        /// Will add the item to the set. If it matches (CompareTo), it'll replace the old value.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Add(IndexedWord<T> item)
        {
            //empty list
            if (_list.First == null)
            {
                _list.AddLast(item);
                return;
            }

            //find its location
            for (LinkedListNode<IndexedWord<T>> node = _list.First; node != null; node = node.Next)
            {
                int compare = item.CompareTo(node.Value);

                // if compare to matches, add after
                if (compare == 0)
                {
                    _list.AddAfter(node, item);
                    return;
                }

                //otherwise if item comes before this
                else if (compare < 0)
                {
                    _list.AddBefore(node, item);
                    return;
                }
            }

            //otherwise it goes at the back
            _list.AddLast(item);
        }

        /// <summary>
        /// Adds the list to the end of the current list
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(IndexedList<T> list)
        {
            foreach (IndexedWord<T> i in list._list)
                _list.AddLast(i);
        }

        public IEnumerator<IndexedWord<T>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }


        private class SeachEnumerator : IEnumerator<T>
        {
            private LinkedList<IndexedWord<T>> _list;
            private LinkedListNode<IndexedWord<T>> _currNode;
            private bool _isDone;
            private string _word;

            public SeachEnumerator(string word, LinkedList<IndexedWord<T>> list)
            {
                _word = word;
                _list = list;
            }

            public T Current
            {
                get
                {
                    return _currNode.Value.Item;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                // Nothing
            }

            public bool MoveNext()
            {
                if (_isDone)
                    return false;

                // Base case for the very first time
                if (_currNode == null)
                {
                    // Pick the first element
                    _currNode = _list.First;
                }

                // Otherwise normal case where we're moving to the next
                else
                {
                    // Move to the next
                    _currNode = _currNode.Next;
                }

                // If there was no next, we've looped over everything
                if (_currNode == null)
                {
                    _isDone = true;
                    return false;
                }

                int comp = compare(_word, _currNode.Value);

                // If matches
                if (comp == 0)
                {
                    return true;
                }

                // If we went to far
                else if (comp > 0)
                {
                    _isDone = true;
                    return false;
                }


                // Fallback cases means we move to the next element
                // - If it matched but was already seen
                // - If it didn't match and we're too early in the list right now

                return MoveNext();
            }

            public void Reset()
            {
                _currNode = null;
                _isDone = false;
            }
        }

        /// <summary>
        /// Uses yield return. This can only ever return matches for a single word, since this object is a list of unassociated words extracted from various items, not a list of words about a single item.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="maxResults"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="intoCombiner"></param>
        public IEnumerable<T> GetMatches(string word)
        {
            return new EnumerableFromEnumerator<T>(new SeachEnumerator(word, _list));
        }

        /// <summary>
        /// Assumes word length is greater than 0.
        /// 
        /// Returns 1 if word is longer than item
        /// 
        /// Same length..
        ///  Returns 1 if word comes after item
        ///  Returns 0 if word matches item
        ///  Returns -1 if word comes before item
        ///  
        /// Word is shorter..
        ///  Returns 1 if word comes after substring
        ///  Returns 0 if word matches substring
        ///  Returns -1 if word comes before substring
        /// </summary>
        /// <param name="word"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static int compare(string word, IndexedWord<T> item)
        {
            if (word.Length > item.Word.Length)
                return -1;

            if (word.Length == item.Word.Length)
                return item.Word.CompareTo(word);

            //use substring
            return item.Word.Substring(0, word.Length).CompareTo(word);
            //if (word.Equals(item.Word.Substring(0, word.Length)))
            //    return 0;

            //return -1;
        }
    }
}
