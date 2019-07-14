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
    /// <summary>
    /// Useful if you only have less than one thousand words to index.
    /// </summary>
    public class BasicIndexer<T>
    {
        public void Index(string stringToIndex, T item, int importance = 0, SplitOptions splitOptions = null)
        {
            if (string.IsNullOrWhiteSpace(stringToIndex))
                return;

            foreach (var word in Splitter.Split(stringToIndex, splitOptions))
                put(new IndexedWord<T>(word, importance, item));
        }

        private MyList<IndexedWord<T>> _words = new MyList<IndexedWord<T>>();

        private void put(IndexedWord<T> item)
        {
            _words.InsertSorted(item);
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
                    cancellationToken.ThrowIfCancellationRequested();

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

        private class MatchesEnumerator : IEnumerator<T>
        {
            private int _currIndex = -1;
            private MyList<IndexedWord<T>> _words;
            private string _word;

            public MatchesEnumerator(string word, MyList<IndexedWord<T>> words)
            {
                _word = word;
                _words = words;
            }

            public T Current
            {
                get
                {
                    return _words[_currIndex].Item;
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
                if (_currIndex == -1)
                {
                    _currIndex = _words.FindIndexForInsert(new IndexedWord<T>(_word, int.MaxValue, default(T)));
                }

                else
                    _currIndex++;


                while (true)
                {
                    if (_currIndex >= _words.Count)
                        return false;

                    var currWord = _words[_currIndex].Word;

                    // If we've gone too far, we're done
                    if (_word.CompareTo(currWord) > 0)
                        return false;

                    if (currWord.StartsWith(_word))
                        return true;

                    _currIndex++;
                }
            }

            public void Reset()
            {
                _currIndex = -1;
            }
        }

        /// <summary>
        /// Enumerates as it goes. Already removed duplicates.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private IEnumerable<T> getMatches(string word)
        {
            if (string.IsNullOrEmpty(word))
                return new T[0];

            return EliminateDuplicates(new EnumerableFromEnumerator<T>(new MatchesEnumerator(word, _words)));
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

        private static IEnumerable<T> EliminateDuplicates(IEnumerable<T> enumerable)
        {
            var enumeratorEliminatingDupes = new EnumeratorEliminatingDuplicates(enumerable.GetEnumerator());

            return new EnumerableFromEnumerator<T>(enumeratorEliminatingDupes);
        }
    }
}
