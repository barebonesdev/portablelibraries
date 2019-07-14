using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolsPortableMost;
using System.Collections.Generic;
using System.Linq;
using ToolsPortable;

namespace ToolsPortableMostTest
{
    [TestClass]
    public class TestSublistSortSameAsParent
    {
        [TestMethod]
        public void Test001()
        {
            MyObservableList<string> people = new MyObservableList<string>()
            {
                "Andrew",
                "Lei",
                "Matt"
            };

            MyObservableList<string> filteredPeople = people.Sublist(i => i.StartsWith("a", StringComparison.CurrentCultureIgnoreCase), SublistSortOption.SameAsParentList);

            AssertListsEqual(new string[] { "Andrew" }, filteredPeople);

            people.Add("Adam");

            /// Andrew
            /// Lei
            /// Matt
            /// Adam

            AssertListsEqual(new string[] { "Andrew", "Adam" }, filteredPeople);

            people.Insert(0, "Andrea");

            /// Andrea
            /// Andrew
            /// Lei
            /// Matt
            /// Adam

            AssertListsEqual(new string[] { "Andrea", "Andrew", "Adam" }, filteredPeople);

            people.Insert(3, "Austin");

            /// Andrea
            /// Andrew
            /// Lei
            /// Austin
            /// Matt
            /// Adam

            AssertListsEqual(new string[] { "Andrea", "Andrew", "Austin", "Adam" }, filteredPeople);

            people.RemoveAt(2);

            /// Andrea
            /// Andrew
            /// Austin
            /// Matt
            /// Adam

            AssertListsEqual(new string[] { "Andrea", "Andrew", "Austin", "Adam" }, filteredPeople);

            people.Insert(2, "Thomas");

            /// Andrea
            /// Andrew
            /// Thomas
            /// Austin
            /// Matt
            /// Adam

            AssertListsEqual(new string[] { "Andrea", "Andrew", "Austin", "Adam" }, filteredPeople);

            people.Insert(0, "Frank");

            /// Frank
            /// Andrea
            /// Andrew
            /// Thomas
            /// Austin
            /// Matt
            /// Adam

            AssertListsEqual(new string[] { "Andrea", "Andrew", "Austin", "Adam" }, filteredPeople);

            people.Add("Steve");

            /// Frank
            /// Andrea
            /// Andrew
            /// Thomas
            /// Austin
            /// Matt
            /// Adam
            /// Steve

            AssertListsEqual(new string[] { "Andrea", "Andrew", "Austin", "Adam" }, filteredPeople);

            people.RemoveAt(1);

            /// Frank
            /// Andrew
            /// Thomas
            /// Austin
            /// Matt
            /// Adam
            /// Steve

            AssertListsEqual(new string[] { "Andrew", "Austin", "Adam" }, filteredPeople);

            people.RemoveAt(3);

            /// Frank
            /// Andrew
            /// Thomas
            /// Matt
            /// Adam
            /// Steve

            AssertListsEqual(new string[] { "Andrew", "Adam" }, filteredPeople);

            people.Clear();

            AssertListsEqual(new string[0], filteredPeople);
        }


        [TestMethod]
        public void Test002()
        {
            MyObservableList<string> people = new MyObservableList<string>();

            MyObservableList<string> filteredPeople = people.Sublist(i => i.StartsWith("a", StringComparison.CurrentCultureIgnoreCase), SublistSortOption.SameAsParentList);

            AssertListsEqual(new string[0], filteredPeople);


            people.Add("Lei");
            people.Add("Andrew");
            people.Add("Matt");

            AssertListsEqual(new string[] { "Andrew" }, filteredPeople);


            people.Add("Thomas");
            people.Add("Steve");

            AssertListsEqual(new string[] { "Andrew" }, filteredPeople);


            people.Add("Adam");

            AssertListsEqual(new string[] { "Andrew", "Adam" }, filteredPeople);


            people.Add("Frank");
            people.Add("Jessica");

            AssertListsEqual(new string[] { "Andrew", "Adam" }, filteredPeople);
        }

        private static void AssertListsEqual(IEnumerable<string> expected, IEnumerable<string> actual)
        {
            Assert.IsTrue(expected.SequenceEqual(actual), "The list was not equal to the expected result.\n\nExpected:\n" + StringTools.ToString(expected, "\n") + "\n\nActual:\n" + StringTools.ToString(actual, "\n"));
        }
    }
}
