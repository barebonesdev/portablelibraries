using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using System.Linq;

namespace HtmlAgilityPack_PCL_Test.Tests
{
    [TestClass]
    public class ComplexSelecting
    {
        private HtmlDocument _doc;

        [TestInitialize]
        public void Initialize()
        {
            _doc = Helper.OpenDocument("Books.xml");
        }

        [TestMethod]
        public void TestSelectAttribute()
        {
            var results = _doc.DocumentNode.SelectNodes("//@id");

            Assert.AreEqual(12, results.Count());

            Assert.AreEqual("bk101", results.First().Attributes["id"].Value);


            results = _doc.DocumentNode.SelectNodes("//*[@id]");

            Assert.AreEqual(12, results.Count());

            Assert.AreEqual("bk101", results.First().Attributes["id"].Value);
        }

        [TestMethod]
        public void TestSelectAttributeValue()
        {
            var results = _doc.DocumentNode.SelectNodes("//*[@*='bk103']");

            Assert.AreEqual(1, results.Count());

            Assert.AreEqual("book", results.First().Name);
            Assert.AreEqual("bk103", results.First().Attributes["id"].Value);
        }

        [TestMethod]
        public void TestSelectAttributeNameAndValue()
        {
            var results = _doc.DocumentNode.SelectNodes("//*[@id='bk103']");

            Assert.AreEqual(1, results.Count());

            Assert.AreEqual("book", results.First().Name);
            Assert.AreEqual("bk103", results.First().Attributes["id"].Value);
        }

        [TestMethod]
        public void SelectTwoLevelsDown()
        {
            var results = _doc.DocumentNode.SelectNodes("//book//price");

            Assert.AreEqual(12, results.Count());
            Assert.AreEqual("44.95", results.First().InnerText);
        }
    }
}
