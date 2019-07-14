using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable.Indexing
{
    internal class IndexedWord<T> : IComparable<IndexedWord<T>>, IComparable
    {
        /// <summary>
        /// Convers the word to lowercase.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="item"></param>
        public IndexedWord(string word, int importance, T item)
        {
            Word = word.ToLower();
            Item = item;
            Importance = importance;
        }

        public string Word { get; private set; }

        public int Importance { get; private set; }

        public T Item { get; private set; }

        public bool Matches(string word)
        {
            return Word.StartsWith(word);
        }

        public int CompareTo(IndexedWord<T> other)
        {
            int comp = Word.CompareTo(other.Word);

            //if this one has greater importance, it'll be first (-1)
            if (comp == 0)
            {
                if (Importance > other.Importance)
                    return -1;

                else if (Importance == other.Importance)
                    return 0;

                return 1;
            }

            return comp;
        }

        public int CompareTo(object obj)
        {
            if (obj is IndexedWord<T>)
                return CompareTo(obj as IndexedWord<T>);

            return 0;
        }
    }
}
