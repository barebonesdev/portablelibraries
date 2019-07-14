using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class WordEnumerator : IEnumerator<string>
    {
        public char Separator { get; private set; }
        private string _str;
        public bool IncludeSeparator { get; private set; }

        /// <summary>
        /// Inclusive, would be the first character of the word
        /// </summary>
        public int Position;

        private string _currentWord = "";

        public WordEnumerator(string str, char separator = ' ', bool includeSeparator = false)
        {
            if (str == null)
                _str = "";

            else
                _str = str;

            Separator = separator;
            IncludeSeparator = includeSeparator;
        }

        /// <summary>
        /// Returns the current word.
        /// </summary>
        public string Current
        {
            get { return _currentWord; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            //the position, non-inclusive (index would be on the character just after our current word)
            Position += _currentWord.Length;

            if (Position >= _str.Length)
            {
                _currentWord = "";
                return false;
            }


            int end = Position;

            //if we're finding a string of separator right now
            if (IncludeSeparator && _str[end++] == Separator)
            {
                //skip till we hit something other than the separator
                for (; end < _str.Length && _str[end] == Separator; end++)
                    ;
            }

            //otherwise we're looking for an actual word
            else
            {
                //skip till we hit a separator, or the end of the string
                for (; end < _str.Length && _str[end] != Separator; end++)
                    ; //nothing
            }

            _currentWord = _str.Substring(Position, end - Position);

            return true;
        }

        public void Reset()
        {
            Position = 0;
            _currentWord = null;
        }

        public void Dispose()
        {
            //nothing
        }
    }
}
