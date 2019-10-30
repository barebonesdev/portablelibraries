using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable.Test
{
    [TestClass]
    public class TestCachedComputation
    {
        private static int m_cachedComputationExecutions;
        private static List<string> m_propertiesChanged = new List<string>();

        [TestMethod]
        public void TestCachedComputation1()
        {
            Clear();
            var obj = new ClassWithComputations();
            obj.PropertyChanged += Obj_PropertyChanged;

            Assert.AreEqual(0, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            // Setting dependent on property shouldn't cause the property to trigger
            obj.Passengers = 2;
            obj.CostPerPassenger = 10;

            Assert.AreEqual(0, m_cachedComputationExecutions);
            AssertHelper.AreSequenceEqual(new string[] { nameof(obj.Passengers), nameof(obj.CostPerPassenger) }, m_propertiesChanged.ToArray());

            Clear();

            // Accessing the property should compute with just one computation and no property changed events
            Assert.AreEqual(20, obj.TotalCostEstimate);
            Assert.AreEqual(1, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Clear();

            // Accessing the property again should have no computations or events
            Assert.AreEqual(20, obj.TotalCostEstimate);
            Assert.AreEqual(0, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Clear();

            // Changing a property should trigger the computation and notification
            obj.Passengers = 3;

            Assert.AreEqual(1, m_cachedComputationExecutions);
            AssertHelper.AreSequenceEqual(new string[] { nameof(obj.Passengers), nameof(obj.TotalCostEstimate) }, m_propertiesChanged.ToArray());

            Clear();

            // Retrieving the value should NOT trigger any computations or notifications
            Assert.AreEqual(30, obj.TotalCostEstimate);
            Assert.AreEqual(0, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Clear();

            // Changing a property that doesn't actually cause a change should trigger computation but NO notification
            obj.CostPerPassenger = 9;

            Assert.AreEqual(1, m_cachedComputationExecutions);
            AssertHelper.AreSequenceEqual(new string[] { nameof(obj.CostPerPassenger) }, m_propertiesChanged.ToArray());

            Clear();

            // Retrieving shouldn't trigger anything
            Assert.AreEqual(30, obj.TotalCostEstimate);
            Assert.AreEqual(0, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Clear();

            // If I change twice, I should only receive one notification since I didn't request the value between these
            obj.Passengers = 4;
            obj.Passengers = 5;

            Assert.AreEqual(1, m_cachedComputationExecutions);
            AssertHelper.AreSequenceEqual(new string[] { nameof(obj.Passengers), nameof(obj.TotalCostEstimate), nameof(obj.Passengers) }, m_propertiesChanged.ToArray());

            Clear();

            // But then when I request, it should be correct, with 1 computation (since dependent changed twice) and no notifications
            Assert.AreEqual(50, obj.TotalCostEstimate);
            Assert.AreEqual(1, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Clear();

            // And when I change again, it should trigger
            obj.Passengers = 6;

            Assert.AreEqual(1, m_cachedComputationExecutions);
            AssertHelper.AreSequenceEqual(new string[] { nameof(obj.Passengers), nameof(obj.TotalCostEstimate) }, m_propertiesChanged.ToArray());
        }

        [TestMethod]
        public void TestCachedComputation2()
        {
            Clear();

            var obj = new ClassWithComputations();
            obj.PropertyChanged += Obj_PropertyChanged;

            Assert.AreEqual(0, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            // Accessing the property should trigger just one computation and no notifications, even if accessed twice
            Assert.AreEqual(0, obj.TotalCostEstimate);
            Assert.AreEqual(1, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Assert.AreEqual(0, obj.TotalCostEstimate);
            Assert.AreEqual(1, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Clear();

            // Changing should trigger computation, but no notification since value still will be 0
            obj.Passengers = 2;

            Assert.AreEqual(1, m_cachedComputationExecutions);
            AssertHelper.AreSequenceEqual(new string[] { nameof(obj.Passengers) }, m_propertiesChanged.ToArray());

            Clear();

            // Setting the other value should trigger computation and notification
            obj.CostPerPassenger = 10;

            Assert.AreEqual(1, m_cachedComputationExecutions);
            AssertHelper.AreSequenceEqual(new string[] { nameof(obj.CostPerPassenger), nameof(obj.TotalCostEstimate) }, m_propertiesChanged.ToArray());

            Clear();

            // Accessing it shouldn't even trigger computation, since it was already computed to send out the notification
            Assert.AreEqual(20, obj.TotalCostEstimate);
            Assert.AreEqual(0, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);

            Clear();

            // Accessing again should trigger nothing
            Assert.AreEqual(20, obj.TotalCostEstimate);
            Assert.AreEqual(0, m_cachedComputationExecutions);
            Assert.AreEqual(0, m_propertiesChanged.Count);
        }

        private static void Clear()
        {
            m_cachedComputationExecutions = 0;
            m_propertiesChanged.Clear();
        }

        private void Obj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            m_propertiesChanged.Add(e.PropertyName);
        }

        private class ClassWithComputations : BindableBase
        {
            private int m_passengers;
            public int Passengers
            {
                get { return m_passengers; }
                set { SetProperty(ref m_passengers, value, nameof(Passengers)); }
            }

            private double m_costPerPassenger;
            public double CostPerPassenger
            {
                get { return m_costPerPassenger; }
                set { SetProperty(ref m_costPerPassenger, value, nameof(CostPerPassenger)); }
            }

            public double TotalCostEstimate => CachedComputation(delegate
            {
                TestCachedComputation.m_cachedComputationExecutions++;

                // We round to values of 10, so that the CostPerPassenger can change without this value changing
                return Passengers * Math.Round(CostPerPassenger / 10) * 10;
            }, TotalCostDependentOn);
            private static readonly string[] TotalCostDependentOn = new string[] { nameof(Passengers), nameof(CostPerPassenger) };

        }
    }
}
