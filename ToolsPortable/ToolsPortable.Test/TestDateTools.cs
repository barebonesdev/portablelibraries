using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToolsPortable.Test
{
    [TestClass]
    public class TestDateTools
    {
        [TestMethod]
        public void TestDifferenceInMonths()
        {
            Assert.AreEqual(0, DateTools.DifferenceInMonths(new DateTime(2018, 1, 15), new DateTime(2018, 1, 7)));
            Assert.AreEqual(0, DateTools.DifferenceInMonths(new DateTime(2018, 2, 1), new DateTime(2018, 1, 7)));
            Assert.AreEqual(0, DateTools.DifferenceInMonths(new DateTime(2018, 2, 6), new DateTime(2018, 1, 7)));
            Assert.AreEqual(1, DateTools.DifferenceInMonths(new DateTime(2018, 2, 7), new DateTime(2018, 1, 7)));
            Assert.AreEqual(1, DateTools.DifferenceInMonths(new DateTime(2018, 2, 8), new DateTime(2018, 1, 7)));
            Assert.AreEqual(1, DateTools.DifferenceInMonths(new DateTime(2018, 3, 1), new DateTime(2018, 1, 7)));
            Assert.AreEqual(-1, DateTools.DifferenceInMonths(new DateTime(2018, 1, 7), new DateTime(2018, 3, 1)));

            Assert.AreEqual(0, DateTools.DifferenceInMonths(new DateTime(2018, 1, 1), new DateTime(2017, 12, 7)));
            Assert.AreEqual(0, DateTools.DifferenceInMonths(new DateTime(2018, 1, 6), new DateTime(2017, 12, 7)));
            Assert.AreEqual(1, DateTools.DifferenceInMonths(new DateTime(2018, 1, 7), new DateTime(2017, 12, 7)));
            Assert.AreEqual(1, DateTools.DifferenceInMonths(new DateTime(2018, 1, 8), new DateTime(2017, 12, 7)));
            Assert.AreEqual(-1, DateTools.DifferenceInMonths(new DateTime(2017, 12, 7), new DateTime(2018, 1, 8)));
        }

        [TestMethod]
        public void TestWithinMonths()
        {
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 1, 15), new DateTime(2018, 1, 7), 1));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 2, 1), new DateTime(2018, 1, 7), 1));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 2, 6), new DateTime(2018, 1, 7), 1));
            Assert.IsFalse(DateTools.WithinMonths(new DateTime(2018, 2, 6), new DateTime(2018, 1, 7), 0));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 2, 7), new DateTime(2018, 1, 7), 2));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 2, 8), new DateTime(2018, 1, 7), 2));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 3, 1), new DateTime(2018, 1, 7), 2));
            Assert.IsFalse(DateTools.WithinMonths(new DateTime(2018, 3, 1), new DateTime(2018, 1, 7), 1));

            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 1, 1), new DateTime(2017, 12, 7), 1));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2017, 12, 7), new DateTime(2018, 1, 1), 1));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 1, 6), new DateTime(2017, 12, 7), 1));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 1, 7), new DateTime(2017, 12, 7), 2));
            Assert.IsTrue(DateTools.WithinMonths(new DateTime(2018, 1, 8), new DateTime(2017, 12, 7), 2));
        }
    }
}
