using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolsPortable.Helpers;

namespace ToolsPortable.Test
{
    [TestClass]
    public class TestLinkDetection
    {
        [TestMethod]
        public void TestNone()
        {
            Assert("No links in here. Just some plain text!", "No links in here. Just some plain text!");
        }

        [TestMethod]
        public void TestJustUrl()
        {
            Assert("msn.com", "[msn.com](http://msn.com)");
            Assert("http://msn.com", "[http://msn.com](http://msn.com)");
            Assert("https://msn.com", "[https://msn.com](https://msn.com)");
            Assert("msn.com/news/latest", "[msn.com/news/latest](http://msn.com/news/latest)");

            Assert("amazon.co.uk", "[amazon.co.uk](http://amazon.co.uk)");
            Assert("amazon.co.uk", "[amazon.co.uk](http://amazon.co.uk)");
        }

        [TestMethod]
        public void TestTrickyUrls()
        {
            Assert("See https://en.wikipedia.org/wiki/List_of_Internet_top-level_domains#C for more info", "See [https://en.wikipedia.org/wiki/List_of_Internet_top-level_domains#C](https://en.wikipedia.org/wiki/List_of_Internet_top-level_domains#C) for more info");
            Assert("More info here: http://jsbin.com/eqocuh/5/edit?html,js,output", "More info here: [http://jsbin.com/eqocuh/5/edit?html,js,output](http://jsbin.com/eqocuh/5/edit?html,js,output)");
            Assert("Simply https://msn.com/u", "Simply [https://msn.com/u](https://msn.com/u)");
        }

        [TestMethod]
        public void TestPunctuation()
        {
            Assert("Visit powerplanner.net.", "Visit [powerplanner.net](http://powerplanner.net).");
            Assert("Visit powerplanner.net!", "Visit [powerplanner.net](http://powerplanner.net)!");
            Assert("Visit https://powerplanner.net.", "Visit [https://powerplanner.net](https://powerplanner.net).");
        }

        [TestMethod]
        public void TestParenthesis()
        {
            Assert("Power Planner (powerplanner.net)", "Power Planner ([powerplanner.net](http://powerplanner.net))");
            Assert("Power Planner (https://powerplanner.net)", "Power Planner ([https://powerplanner.net](https://powerplanner.net))");
        }

        [TestMethod]
        public void TestEndingSlash()
        {
            Assert("Visit https://msn.com/ for more info", "Visit [https://msn.com/](https://msn.com/) for more info");
            Assert("See powerplanner.net/login/ to login", "See [powerplanner.net/login/](http://powerplanner.net/login/) to login");
            Assert("Learn more at http://google.com/.", "Learn more at [http://google.com/](http://google.com/).");
        }

        [TestMethod]
        public void TestSubdomains()
        {
            Assert("app.powerplanner.net", "[app.powerplanner.net](http://app.powerplanner.net)");
            Assert("https://app.powerplanner.net", "[https://app.powerplanner.net](https://app.powerplanner.net)");
        }

        [TestMethod]
        public void TestDashesInDomains()
        {
            Assert("powerplannerapp-staging.net", "[powerplannerapp-staging.net](http://powerplannerapp-staging.net)");
            Assert("powerplannerapp-staging.azurewebsites.net", "[powerplannerapp-staging.azurewebsites.net](http://powerplannerapp-staging.azurewebsites.net)");
        }

        [TestMethod]
        public void TestEmails()
        {
            Assert("ben@gmail.com", "[ben@gmail.com](mailto:ben@gmail.com)");
            Assert("andrew-leader@outlook.com", "[andrew-leader@outlook.com](mailto:andrew-leader@outlook.com)");
            Assert("andrew.newman@outlook.com", "[andrew.newman@outlook.com](mailto:andrew.newman@outlook.com)");
        }

        [TestMethod]
        public void TestEmailsInText()
        {
            Assert("Contact ben@gmail.com for help", "Contact [ben@gmail.com](mailto:ben@gmail.com) for help");
        }

        [TestMethod]
        public void TestEmailsWithPunctuation()
        {
            Assert("Contact ben@gmail.com!", "Contact [ben@gmail.com](mailto:ben@gmail.com)!");
            Assert("Contact ben@gmail.com.", "Contact [ben@gmail.com](mailto:ben@gmail.com).");
            Assert("Ben (ben@gmail.com)", "Ben ([ben@gmail.com](mailto:ben@gmail.com))");
        }

        private static void Assert(string input, string expected)
        {
            var runs = LinkDetectionHelper.DetectRuns(input);

            string actual = "";

            foreach (var run in runs)
            {
                if (run is PortableHyperlinkRun hl)
                {
                    actual += $"[{run.Text}]({hl.Uri.OriginalString})";
                }
                else
                {
                    actual += run.Text;
                }
            }

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expected, actual, "Original: " + input);
        }
    }
}
