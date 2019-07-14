using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using System.Linq;

namespace HtmlAgilityPack_PCL_Test.Tests
{
    [TestClass]
    public class CraigslistPostListing
    {
        private HtmlDocument _doc;

        [TestInitialize]
        public void Initialize()
        {
            _doc = Helper.OpenDocument("CraigslistPostListing.xml");
        }

        private string getValue(string xpath)
        {
            return _doc.DocumentNode.SelectSingleNode(xpath).InnerText.Trim();
        }

        [TestMethod]
        public void TestTitle()
        {
            Assert.AreEqual("2013 Ford Mustang Boss 302", getValue("//title"));
        }

        [TestMethod]
        public void TestNoEmailMatch()
        {
            HtmlNode node = _doc.DocumentNode.SelectSingleNode("//*[@class='replybelow']");

            //should be null, since this one has a contact
            Assert.IsNull(node);
        }

        [TestMethod]
        public void TestBodyMatch()
        {
            string bodyText = getValue("//*[@*='postingbody']");

            Assert.IsTrue(bodyText.StartsWith("Like new Gotta Have it"));
        }

        [TestMethod]
        public void TestAttributes()
        {
            var results = _doc.DocumentNode.SelectNodes("//*[@*='attrgroup']//span");

            //there should be five attributes like odometer, model name, transmission, etc
            Assert.AreEqual(5, results.Count());

            Assert.AreEqual("condition: like new", results.First().InnerText.Trim());
            Assert.AreEqual("2013 Ford Mustang", results.ElementAt(1).InnerText.Trim());
        }

        [TestMethod]
        public void TestDataLatitude()
        {
            HtmlNode node = _doc.DocumentNode.SelectSingleNode("//@data-latitude");

            Assert.AreEqual("32.083900", node.Attributes["data-latitude"].Value);
        }
    }
}
