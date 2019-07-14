using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public static class MyCollections
    {
        /// <summary>
        /// NOT FINISHED
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalSource"></param>
        /// <returns></returns>
        public static List<T[]> GenerateAllCombinations<T>(this IEnumerable<T> originalSource)
        {
            //this is one of the combinations, so now we have an array with no additional work!
            T[] source = originalSource.ToArray();

            int numOfCombinations = (int)Math.Pow(2, source.Length) - 1;

            List<T[]> answer = new List<T[]>(numOfCombinations);


            //for (int i = 1; i < source.Length; i++)
            //    generateAllCombinationsHelper(source, answer, 0, 0, i);


            //add the final size array
            answer.Add(source);

            return answer;
        }

        private static void generateAllCombinationsHelper<T>(T[] source, List<T[]> answer, int start, int level)
        {
            for (int i = 0; i < source.Length; i++)
            {

            }
        }
    }
}
