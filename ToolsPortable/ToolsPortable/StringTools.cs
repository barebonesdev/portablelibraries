using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class StringTools
    {
        public static readonly char[] INVALID_FILE_NAME_CHARS = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|', ' ' };

        public static string CreateSafeFileName(string originalFileName)
        {
            StringBuilder sb = new StringBuilder(originalFileName.Length);

            for (int i = 0; i < originalFileName.Length && i < 256; i++)
            {
                char c = originalFileName[i];

                if (!INVALID_FILE_NAME_CHARS.Contains(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public static bool IsStringUrlSafe(string str)
        {
            for (int i = 0; i < str.Length; i++)
                if (!IsCharUrlSafe(str[i]))
                    return false;

            return true;
        }

        public static bool IsStringFilenameSafe(string str)
        {
            for (int i = 0; i < str.Length; i++)
                if (!isCharFilenameSafe(str[i]))
                    return false;

            return true;
        }

        public static readonly char[] VALID_SPECIAL_FILENAME_CHARS = new char[] { '$', '-', '_', '.', '+', '!', '\'', '(', ')', ',' };

        private static bool isCharFilenameSafe(char c)
        {
            if ((c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                (c >= '0' && c <= '9') ||
                VALID_SPECIAL_FILENAME_CHARS.Contains(c))
                return true;

            return false;
        }

        public static readonly char[] VALID_SPECIAL_URL_CHARS = new char[] { '$', '-', '_', '.', '+', '!', '*', '\'', '(', ')', ',' };

        private static bool IsCharUrlSafe(char c)
        {
            if ((c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                (c >= '0' && c <= '9') ||
                VALID_SPECIAL_URL_CHARS.Contains(c))
                return true;

            return false;
        }

        public static string ToString(IEnumerable items, string separator)
        {
            StringBuilder answer = new StringBuilder();

            IEnumerator i = items.GetEnumerator();

            while (i.MoveNext())
                answer.Append(i.Current.ToString()).Append(separator);

            if (answer.Length > 0)
                answer.Remove(answer.Length - separator.Length, separator.Length);

            return answer.ToString();
        }

        public static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        public static bool IsSpace(char c)
        {
            return c == ' ' || c == '\n' || c == '\t' || c == '\r';
        }

        public static bool StartsWith(string str, int posInStr, string startsWith)
        {
            for (int i = 0; i < startsWith.Length; i++, posInStr++)
            {
                if (posInStr >= str.Length)
                    return false;

                if (str[posInStr] != startsWith[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a string without any ending whitespace characters (IsSpace())
        /// </summary>
        /// <returns></returns>
        public static string TrimEnd(string str)
        {
            if (str.Length > 0 && IsSpace(str[str.Length - 1]))
            {
                for (int i = str.Length - 2; i >= 0; i--)
                    if (!IsSpace(str[i]))
                        return str.Substring(0, i + 1);

                return "";
            }

            return str;
        }

        public static string TrimLengthWithEllipses(string str, int maxLength)
        {
            if (str == null || str.Length < maxLength)
            {
                return str;
            }

            return str.Substring(0, maxLength).TrimEnd() + "...";
        }

        public static string TrimLength(string str, int maxLength)
        {
            if (str == null || str.Length < maxLength)
            {
                return str;
            }

            return str.Substring(0, maxLength).TrimEnd();
        }

        private static int grab(string str, int startAt, string pattern, string[] arr)
        {
            int found = 0;
            int x = 0;
            string workingStr = null;

            //try to match
            for (int i = startAt; i < str.Length; i++)
            {
                //if reached end
                if (x == pattern.Length || (x == pattern.Length - 1 && pattern[x] == '*'))
                {
                    //if we matched all the words
                    if (found == arr.Length)
                        return i;

                    //otherwise we'll check the next line
                    else
                        return -1;
                }

                //if matching *
                if (pattern[x] == '*')
                {
                    //if done with asterix match
                    if (doneWithMatch(str, pattern, x, i))
                        x++;

                    //otherwise stay at asterix match
                    else
                        continue;
                }

                //if matching ~
                if (pattern[x] == '~')
                {
                    //if new string
                    if (workingStr == null)
                        workingStr = "";

                    //if the char is the finisher for the match
                    if (doneWithMatch(str, pattern, x, i))
                    {
                        //place in the string
                        arr[found++] = workingStr;

                        //reset string to null
                        workingStr = null;

                        //move forward
                        x++;
                    }

                    else
                    {
                        //append
                        workingStr += str[i];

                        continue;
                    }
                }


                //if character matches
                if (pattern[x] == str[i])
                    x++;

                else
                    break;
            }

            //if reached end
            if (x == pattern.Length || (x == pattern.Length - 1 && pattern[x] == '*'))
            {
                //if we matched all the words
                if (found == arr.Length)
                    return str.Length;
            }

            return -1;
        }

        public static int Grab(string str, int startAt, string pattern, string[] arr)
        {
            return grab(str, startAt, pattern, arr);
        }

        /// <summary>
        /// "~" is the character for finding the word, "*" will match as many characters as necessary
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startAt"></param>
        /// <param name="pattern"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string[] Grab(string str, int startAt, string pattern, int number)
        {
            string[] arr = new string[number];

            for (; startAt < str.Length; startAt++)
                if (grab(str, startAt, pattern, arr) != -1)
                    return arr;

            return null;
        }

        private static bool doneWithMatch(string _str, string format, int x, int _i)
        {
            int i = _i;

            for (x++; x < format.Length && format[x] != '*' && format[x] != '~'; x++, i++)
            {
                if (i == _str.Length || _str[i] == '\n')
                    return false;

                if (format[x] != _str[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Replaces \r breaks with \n breaks
        /// </summary>
        /// <param name="originalString"></param>
        /// <returns></returns>
        public static string NormalizeLineBreaks(string originalString)
        {
            if (originalString == null || originalString.Length == 0)
            {
                return originalString;
            }

            return originalString.Replace("\r", "\n");
        }
    }

    public class StringIterator
    {
        private string _str;
        private int _i = 0;

        public int GetI
        {
            get { return _i; }
        }

        public StringIterator(string str)
        {
            _str = str;
        }

        private bool doneWithMatch(string format, int x)
        {
            int i = _i;

            for (x++; x < format.Length && format[x] != '*' && format[x] != '~'; x++, i++)
            {
                if (i == _str.Length || _str[i] == '\n')
                    return false;

                if (format[x] != _str[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Will look for the line that matches the format and grab the words from it.
        /// 
        /// "*" will match any number of characters between.
        /// "~" will be the string to get.
        /// 
        /// If couldn't match enough words, returns null.
        /// 
        /// BUGS: Probably doesn't work trying to get a string at the end, like "words~". A "~" MUST be surrounded by characters and NOT "*".
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string[] GrabWords(string format, int numOfWords, bool onlyNextLine = false)
        {
            int oldI = _i;

            string[] arr = new string[numOfWords];

            while (advance())
            {
                int x = 0;
                string str = null;

                //try to match
                for (; _i < _str.Length && _str[_i] != '\n'; _i++)
                {
                    //if reached end
                    if (x == format.Length || (x == format.Length - 1 && format[x] == '*'))
                    {
                        //if we matched all the words
                        if (arr[arr.Length - 1] != null)
                            return arr;

                        //otherwise we'll check the next line
                        else
                            break;
                    }

                    //if matching *
                    if (format[x] == '*')
                    {
                        //if done with asterix match
                        if (doneWithMatch(format, x))
                            x++;

                        //otherwise stay at asterix match
                        else
                            continue;
                    }

                    //if matching ~
                    if (format[x] == '~')
                    {
                        //if new string
                        if (str == null)
                            str = "";

                        //if the char is the finisher for the match
                        if (doneWithMatch(format, x))
                        {
                            //place in the string
                            for (int y = 0; y < arr.Length; y++)
                                if (arr[y] == null)
                                {
                                    arr[y] = str;
                                    break;
                                }

                            //reset string to null
                            str = null;

                            //move forward
                            x++;
                        }

                        else
                        {
                            //append
                            str += _str[_i];

                            continue;
                        }
                    }


                    //if character matches
                    if (format[x] == _str[_i])
                        x++;

                    else
                        break;
                }

                if (onlyNextLine)
                {
                    _i = oldI;
                    return null;
                }

                //reset array
                for (int y = 0; y < arr.Length && arr[y] != null; y++)
                    arr[y] = null;
            }

            _i = oldI;
            return null;
        }

        public string GrabWord(string format, bool onlyNextLine = false)
        {
            string[] arr = GrabWords(format, 1, onlyNextLine);

            if (arr == null)
                return null;

            return arr[0];
        }

        private bool advance()
        {
            for (; _i < _str.Length; _i++)
            {
                //if we found a next line character
                if (_str[_i] == '\n' || _i == 0)
                {
                    //advance forward till we're not at a newline/whitespace
                    for (; _i < _str.Length; _i++)
                        if (_str[_i] != '\n' && _str[_i] != ' ' && _str[_i] != '\t' && _str[_i] != '\r')
                            return true;
                }
            }

            return false;
        }
    }
}
