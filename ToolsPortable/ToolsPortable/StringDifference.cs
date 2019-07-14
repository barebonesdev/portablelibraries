using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class StringDifference
    {
        public LinkedList<Compressed> String { get; private set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (Compressed c in String)
            {
                switch (c.ChangeType)
                {
                    case Change.Delete:
                        builder.Append('[');
                        builder.Append(c.String);
                        builder.Append(']');
                        break;


                    case Change.Insert:
                        builder.Append('{');
                        builder.Append(c.String);
                        builder.Append('}');
                        break;

                    case Change.Move:
                        builder.Append('\'');
                        builder.Append(c.String);
                        builder.Append('\'');
                        break;

                    default:
                        builder.Append(c.String);
                        break;
                }
            }

            return builder.ToString();
        }

        private StringDifference()
        {

        }

        public enum Change { Delete, Insert, None, Move }

        public class Compressed
        {
            public Compressed(string str, Change change)
            {
                String = str;
                ChangeType = change;
            }

            public string String { get; private set; }

            public Change ChangeType { get; private set; }
        }

        private class Word
        {
            public string ActualWord;

            public List<int> Positions;

            public int FinalPosition = -1;

            public bool IsCondensable { get { return FinalPosition == -1 && Positions != null; } }
        }

        public static StringDifference Analyze(string oldString, string newString)
        {
            Dictionary<string, List<int>> originalWords = new Dictionary<string, List<int>>();

            //look at words of old string, separated by spaces, including spaces as a word
            WordEnumerator enumerator = new WordEnumerator(oldString, ' ', true);
            int position;
            for (position = 1; enumerator.MoveNext(); position++)
            {
                List<int> list = null;
                if (!originalWords.TryGetValue(enumerator.Current, out list))
                {
                    list = new List<int>();
                    originalWords.Add(enumerator.Current, list);
                }

                list.Add(position);
            }




            //mark all the potential old positions of words in the new string
            enumerator = new WordEnumerator(newString, ' ', true);
            List<Word> words = new List<Word>(position)
            {
                //starts with the beginning empty blank
                new Word()
                {
                    ActualWord = "",
                    Positions = new List<int>(1) { 0 }
                }
            };

            while (enumerator.MoveNext())
            {
                List<int> list = null;
                originalWords.TryGetValue(enumerator.Current, out list);

                words.Add(new Word()
                {
                    ActualWord = enumerator.Current,
                    Positions = list
                });
            }

            //add the end blank
            words.Add(new Word()
            {
                ActualWord = "",
                Positions = new List<int>(1) { position }
            });





            LongestPair longestPair;
            int start = 0;

            bool[] usedPositions = new bool[position + 1];

            do
            {
                longestPair = null;

                //keep track of where actual condensable items start
                for (; start < words.Count && !words[start].IsCondensable; start++)
                    ; //nothing

                //loop through, looking for the longest run
                for (int i = start; i < words.Count; i++)
                {
                    if (words[i].IsCondensable)
                    {
                        LongestPair pair = findLongest(words, i, usedPositions);

                        if (pair != null)
                            if (longestPair == null || pair.Length > longestPair.Length)
                                longestPair = pair;
                    }

                }

                //condense
                if (longestPair != null)
                {
                    int pos = longestPair.Position;

                    for (int i = longestPair.StartIndex; i <= longestPair.EndIndex; i++)
                    {
                        usedPositions[pos] = true;
                        words[i].FinalPosition = pos++;
                    }
                }

            } while (longestPair != null);




            StringDifference answer = new StringDifference()
            {
                String = new LinkedList<Compressed>()
            };

            enumerator = new WordEnumerator(oldString, ' ', true);

            int y = 1; //skip first blank (0) in words
            position = 1; //first actual word counts as position 1


            for (; y < words.Count; y++)
            {
                StringBuilder builder = new StringBuilder();

                //if new words
                if (words[y].FinalPosition == -1)
                {
                    for (; y < words.Count && words[y].FinalPosition == -1; y++)
                        builder.Append(words[y].ActualWord);

                    if (builder.Length > 0)
                    {
                        answer.String.AddLast(new Compressed(builder.ToString(), Change.Insert));
                        builder = new StringBuilder();
                    }

                    y--;
                }

                //else if deleted words
                else if (words[y].FinalPosition > position)
                {
                    for (; words[y].FinalPosition > position && enumerator.MoveNext(); position++)
                        if (!usedPositions[position])
                            builder.Append(enumerator.Current);

                    if (builder.Length > 0)
                    {
                        answer.String.AddLast(new Compressed(builder.ToString(), Change.Delete));
                        builder = new StringBuilder();
                    }
                }

                //else if matched words
                else if (words[y].FinalPosition == position && enumerator.MoveNext())
                {
                    builder.Append(enumerator.Current);
                    y++;
                    position++;

                    for (; y < words.Count && words[y].FinalPosition == position && enumerator.MoveNext(); y++, position++)
                        builder.Append(enumerator.Current);

                    if (builder.Length > 0)
                    {
                        answer.String.AddLast(new Compressed(builder.ToString(), Change.None));
                        builder = new StringBuilder();
                    }

                    y--;
                }

                //moved words
                else if (words[y].FinalPosition < position)
                {
                    int prevPos = words[y].FinalPosition - 1;

                    for (; y < words.Count && words[y].FinalPosition < position && words[y].FinalPosition == prevPos + 1; y++, prevPos++)
                        builder.Append(words[y].ActualWord);

                    if (builder.Length > 0)
                    {
                        answer.String.AddLast(new Compressed(builder.ToString(), Change.Move));
                        builder = new StringBuilder();
                    }

                    y--;
                }
            }


            return answer;
        }

        private class LongestPair
        {
            public int StartIndex;

            /// <summary>
            /// Inclusive
            /// </summary>
            public int EndIndex;

            /// <summary>
            /// The position to start condensing by
            /// </summary>
            public int Position;

            public int Length
            {
                get { return EndIndex - StartIndex; }
            }
        }

        private static LongestPair findLongest(List<Word> words, int i, bool[] usedPositions)
        {
            int longestEnd = 0;
            int pos = 0;
            bool found = false;

            foreach (int x in words[i].Positions)
            {
                //if the position hasn't been used
                if (!usedPositions[x])
                {
                    int end = findEnd(words, i + 1, x + 1, usedPositions);

                    if (end > longestEnd)
                    {
                        longestEnd = end;
                        pos = x;
                    }

                    found = true;
                }
            }

            if (found)
                return new LongestPair()
                {
                    StartIndex = i,
                    EndIndex = longestEnd,
                    Position = pos
                };

            return null;
        }

        private static int findEnd(List<Word> words, int i, int neededPosition, bool[] usedPositions)
        {
            if (i >= words.Count || //at end of list
                !words[i].IsCondensable ||
                usedPositions[neededPosition]) //not condensable
                return i - 1;

            //pick the one that could have followed
            foreach (int y in words[i].Positions)
                if (neededPosition == y)
                {
                    //try to go further
                    return findEnd(words, i + 1, y + 1, usedPositions);
                }

            //nothing could have followed, discontinuity
            return i - 1;
        }
    }
}
