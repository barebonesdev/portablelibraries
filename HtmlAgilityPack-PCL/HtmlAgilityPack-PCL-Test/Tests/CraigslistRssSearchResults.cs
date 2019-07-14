using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using System.Linq;

namespace HtmlAgilityPack_PCL_Test.Tests
{
    [TestClass]
    public class CraigslistRssSearchResults
    {
        private HtmlDocument _doc;

        private HtmlNode getFirstItem()
        {
            return _doc.DocumentNode.SelectSingleNode("//item");
        }

        [TestInitialize]
        public void Initialize()
        {
            _doc = Helper.OpenDocument("CraigslistRssSearchResults.xml");
        }

        [TestMethod]
        public void TestHas25Items()
        {
            HtmlNode[] items = _doc.DocumentNode.SelectNodes("//item").ToArray();


            Assert.AreEqual(25, items.Length);
        }

        [TestMethod]
        public void TestFirstTitle()
        {
            HtmlNode node = getFirstItem().SelectSingleNode(".//title");
            string title = node.InnerText;

            Assert.AreEqual("2007 4Runner SR5 $13500", title);
        }

        [TestMethod]
        public void TestFirstLink()
        {
            string link = getFirstItem().SelectSingleNode(".//link").InnerText;

            Assert.AreEqual("http://tucson.craigslist.org/cto/4520917959.html", link);
        }

        [TestMethod]
        public void TestFirstDate()
        {
            string date = getFirstItem().SelectSingleNode("//dc:date").InnerText;

            Assert.AreEqual("2014-06-14T15:55:30-07:00", date);
        }

        [TestMethod]
        public void TestFirstImage()
        {
            string image = getFirstItem().SelectSingleNode("//enc:enclosure").Attributes["resource"].Value;

            Assert.AreEqual("http://images.craigslist.org/00q0q_7i4NmC5TZLp_300x300.jpg", image);
        }
    }
}
