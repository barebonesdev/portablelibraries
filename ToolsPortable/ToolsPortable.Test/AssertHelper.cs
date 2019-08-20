using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable.Test
{
    public class AssertHelper
    {
        public static void AreSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            if (expected == null)
            {
                Assert.Fail("Sequence not equal. Expected: null. Actual: " + string.Join(",", actual));
            }

            if (actual == null)
            {
                Assert.Fail("Sequence not equal. Expected: " + string.Join(",", expected) + ". Actual: null");
            }

            if (!expected.SequenceEqual(actual))
            {
                Assert.Fail("Sequence not equal. Expected: " + string.Join(",", expected) + ". Actual: " + string.Join(",", actual));
            }
        }
    }
}
