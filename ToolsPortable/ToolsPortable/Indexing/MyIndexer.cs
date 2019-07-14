using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsPortable.Indexing
{
    public class MyIndexer<T>
    {
        private IndexedList<T> _shortItems = new IndexedList<T>();

        private IndexedList<T>[] _items = new IndexedList<T>[50653];

        /// <summary>
        /// Generates a list of all the possible search strings
        /// </summary>
        /// <returns></returns>
        public HashSet<string> GetAllStrings()
        {
            HashSet<string> set = new HashSet<string>();
            addRange(_shortItems.List, set);

            for (int i = 0; i < _items.Length; i++)
                if (_items[i] != null)
                    addRange(_items[i].List, set);

            return set;
        }

        private void addRange(LinkedList<IndexedWord<T>> list, HashSet<string> addTo)
        {
            LinkedList<IndexedWord<T>>.Enumerator e = list.GetEnumerator();
            while (e.MoveNext())
                addTo.Add(e.Current.Word);
        }

        public List<T> GetMatches(string sentence, int maxResults, CancellationToken cancellationToken)
        {
            try
            {
                sentence = sentence.ToLower();
                
                // Get the words form the search string sentence
                string[] split = Splitter.Split(sentence).ToArray();

                // TODO: We need to merge based on items that have ALL the words, not just some
                // We actually need to provide the array of words, and then only insert an item in the
                // combiner if ALL the words matched.

                string firstWord = split.FirstOrDefault();
                string[] subsequentWords = split.Skip(1).ToArray();

                if (firstWord == null)
                    return new List<T>();

                List<T> answer = new List<T>();

                // PROBLEM: All these yield returns result in stack overflows!!!

                foreach (var match in getMatches(firstWord))
                {
                    // If subsequent words match, it's a match
                    if (DoSubsequentWordsMatch(match, subsequentWords))
                    {
                        answer.Add(match);

                        if (answer.Count >= maxResults)
                            return answer;
                    }
                }

                return answer;
            }

            catch (OperationCanceledException)
            {
                throw;
            }

            catch (Exception e)
            {
                Debug.WriteLine("GetMatches exception: " + e.ToString());
                return new List<T>();
            }
        }


        private bool DoSubsequentWordsMatch(T item, IEnumerable<string> subsequentWords)
        {
            string nextWord = subsequentWords.FirstOrDefault();

            if (nextWord == null)
                return true;

            // If we can find a match for the next word, for this item...
            if (getMatches(nextWord).Any(i => object.ReferenceEquals(i, item)))
            {
                // Then make sure subsequent words find a match too
                return DoSubsequentWordsMatch(item, subsequentWords.Skip(1));
            }

            return false;
        }

        private class EnumeratorEliminatingDuplicates : IEnumerator<T>
        {
            private IEnumerator<T> _sourceEnumerator;
            private HashSet<T> _seen = new HashSet<T>();

            public EnumeratorEliminatingDuplicates(IEnumerator<T> sourceEnumerator)
            {
                _sourceEnumerator = sourceEnumerator;
            }

            public T Current
            {
                get
                {
                    return _sourceEnumerator.Current;
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
                while (true)
                {
                    if (_sourceEnumerator.MoveNext())
                    {
                        bool isNew = _seen.Add(_sourceEnumerator.Current);

                        if (isNew)
                            return true;
                    }

                    else
                        return false;
                }
            }

            public void Reset()
            {
                _seen = new HashSet<T>();
                _sourceEnumerator.Reset();
            }
        }

        private class AllEnumerator : IEnumerator<T>
        {
            private IndexedList<T>[] _items;
            private int _endIndexNotInclusive;
            private int _currIndex = -1;
            private int _startIndex;
            private IEnumerator<IndexedWord<T>> _currEnumerator;
            private bool _isDone;

            public AllEnumerator(IndexedList<T>[] items, int startIndex, int endIndexNotInclusive)
            {
                _items = items;
                _endIndexNotInclusive = endIndexNotInclusive;
                _startIndex = startIndex;

                Reset();
            }

            public T Current
            {
                get
                {
                    return _currEnumerator.Current.Item;
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
                while (true)
                {
                    if (_isDone)
                        return false;

                    // If we have an enumerator, and it has next
                    if (_currEnumerator != null && _currEnumerator.MoveNext())
                        return true;

                    // Otherwise, we didn't have an enumerator or we need to go to the next list
                    while (true)
                    {
                        _currIndex++;

                        if (_currIndex >= _endIndexNotInclusive)
                        {
                            _isDone = true;
                            return false;
                        }

                        var list = _items[_currIndex];

                        if (list != null)
                        {
                            _currEnumerator = list.GetEnumerator();
                            break;
                        }
                    }
                }
            }

            public void Reset()
            {
                _currIndex = _startIndex - 1;
                _currEnumerator = null;
                _isDone = false;
            }
        }

        private static IEnumerable<T> EliminateDuplicates(IEnumerable<T> enumerable)
        {
            var enumeratorEliminatingDupes = new EnumeratorEliminatingDuplicates(enumerable.GetEnumerator());

            return new EnumerableFromEnumerator<T>(enumeratorEliminatingDupes);
        }

        /// <summary>
        /// Uses yield return. Doesn't have any dupes, they've already been eliminated.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private IEnumerable<T> getMatches(string word)
        {
            if (string.IsNullOrEmpty(word))
                return new T[0];

            //regular item
            if (word.Length >= 3)
            {
                int index = getIndex(word);

                IndexedList<T> f = _items[index];
                if (f == null)
                    return new T[0];

                return EliminateDuplicates(f.GetMatches(word));
            }

            //SHORT ITEM
            //get all the short matches

            var shortMatches = _shortItems.GetMatches(word);

            //now add all possibilities
            int start = getIndex(word);
            int end = start;
            end += word.Length == 1 ? 1369 : 37;

            var allPossibilities = new EnumerableFromEnumerator<T>(new AllEnumerator(_items, start, end));

            var merged = shortMatches.Concat(allPossibilities);

            return EliminateDuplicates(merged);
        }

        private IndexedWord<T> get(LinkedList<IndexedWord<T>> list, string word)
        {
            foreach (IndexedWord<T> i in list)
                if (i.Word.StartsWith(word))
                    return i;

            return null;
        }

        public void Index(string stringToIndex, T item, int importance = 0, SplitOptions splitOptions = null)
        {
            if (string.IsNullOrWhiteSpace(stringToIndex))
                return;

            stringToIndex = stringToIndex.ToLower();

            foreach (var word in Splitter.Split(stringToIndex, splitOptions))
                put(new IndexedWord<T>(word, importance, item));
        }

        private void put(IndexedWord<T> item)
        {
            //if short word
            if (item.Word.Length < 3)
                _shortItems.Add(item);

            //otherwise add in array
            else
            {
                int index = getIndex(item.Word);

                IndexedList<T> list = _items[index];

                if (list == null)
                {
                    list = new IndexedList<T>();
                    _items[index] = list;
                }

                list.Add(item);
            }
        }

        /// <summary>
        /// word must not have any spaces, and should be at least be length of 3.
        /// Returns word[0] * 37 * 37 + word[1] * 37 + word[2]
        /// 
        /// If it's a short word, it returns where it would start.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private static int getIndex(string word)
        {
            if (word.Length < 3)
            {
                if (word.Length == 1)
                    return getIndex(word[0]) * 1369;

                return getIndex(word[0]) * 1369 + getIndex(word[1]) * 37;
            }

            return getIndex(word[0]) * 1369 + getIndex(word[1]) * 37 + getIndex(word[2]);
        }

        /// <summary>
        /// 'a' is 0, 'z' is 25, '0' is 26, '9' is 35, others are 36
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static int getIndex(char c)
        {
            if (c >= 'a' && c <= 'z')
                return c - 'a';

            if (c >= 'A' && c <= 'Z')
                return c - 'A';

            if (c >= '0' && c <= '9')
                return 26 + c - '0';

            return 36;
        }
    }
}
