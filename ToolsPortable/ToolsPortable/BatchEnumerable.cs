using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public static class BatchEnumerable
    {
        /// <summary>
        /// Batches the source sequence into sized buckets.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="size">Size of buckets.</param>
        /// <returns>A sequence of equally sized buckets (except the last) containing elements of the source collection.</returns>
        /// <remarks> This operator uses deferred execution and streams its results (buckets and bucket content).</remarks>
        public static IEnumerable<TSource[]> BatchAsArrays<TSource>(this IEnumerable<TSource> source, int size)
        {
            return BatchAsArraysImpl(source, size);
        }

        private static IEnumerable<TSource[]> BatchAsArraysImpl<TSource>(this IEnumerable<TSource> source, int size)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                {
                    bucket = new TSource[size];
                }

                bucket[count++] = item;

                // The bucket is fully buffered before it's yielded
                if (count != size)
                {
                    continue;
                }

                // Select is necessary so bucket contents are streamed too
                yield return bucket;

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                // Incomplete bucket so trim it
                if (count < size)
                {
                    bucket = bucket.Take(count).ToArray();
                }

                yield return bucket;
            }
        }
    }
}