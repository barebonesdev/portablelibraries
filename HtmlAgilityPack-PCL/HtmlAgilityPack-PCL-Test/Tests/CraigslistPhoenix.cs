using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using System.Linq;

namespace HtmlAgilityPack_PCL_Test.Tests
{
    [TestClass]
    public class CraigslistPhoenix
    {
        private HtmlDocument _doc;

        [TestInitialize]
        public void Initialize()
        {
            _doc = Helper.OpenDocument("CraigslistPhoenix.xml");
        }

        [TestMethod]
        public void TestInnerCityTags()
        {
            var results = _doc.DocumentNode.SelectNodes("//*[@*='sublinks']//a[@href][@title]");

            Assert.AreEqual(4, results.Count(), "Number of tags");

            Assert.AreEqual("cph", results.First().InnerText);
            Assert.AreEqual("central/south phx", results.First().Attributes["title"].Value);
            Assert.AreEqual("/cph/", results.First().Attributes["href"].Value);

            Assert.AreEqual("wvl", results.Last().InnerText);
            Assert.AreEqual("west valley", results.Last().Attributes["title"].Value);
            Assert.AreEqual("/wvl/", results.Last().Attributes["href"].Value);
        }
    }
}
