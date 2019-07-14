using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable.Indexing
{
    internal static class Splitter
    {
        private static SplitOptions DefaultOptions = new SplitOptions();

        /// <summary>
        /// Removes empty strings
        /// </summary>
        /// <param name="str"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IEnumerable<string> Split(string str, SplitOptions options = null)
        {
            string[] split = str.Split(' ', '\n', '\t', '\r');

            var withoutEmpty = split.Where(i => !string.IsNullOrEmpty(i));
            
            return ProcessOptions(withoutEmpty, options == null ? DefaultOptions : options);
        }

        private static IEnumerable<string> ProcessOptions(IEnumerable<string> splitWithoutEmpties, SplitOptions options)
        {
            foreach (var s in splitWithoutEmpties)
            {
                if (s.Equals("&"))
                {
                    yield return "and";
                    continue;
                }

                if (s.Equals(".") || s.Equals(","))
                    continue;

                else
                    yield return s;

                if (options.SplitUnderscores)
                {
                    string[] subSplit = s.Split('_');

                    foreach (var subS in subSplit.Where(i => !string.IsNullOrEmpty(i)))
                        yield return subS;
                }
            }
        }
    }
}
