using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable.Test
{
    [TestClass]
    public class TestAsyncWorkerQueue
    {
        [TestMethod]
        public async Task TestSimpleAsyncQueue()
        {
            var simpleQueue = new SimpleAsyncWorkerQueue<int>();

            var first = simpleQueue.QueueAsync(async delegate
            {
                await Task.Delay(1000);

                return 1;
            });

            var second = simpleQueue.QueueAsync(async delegate
            {
                await Task.Delay(1000);

                return 2;
            });

            await Task.Delay(1500);
            Assert.AreEqual(1, first.Result);

            await Task.Delay(500);
            Assert.IsFalse(second.IsCompleted);
            await Task.Delay(500);
            Assert.AreEqual(2, second.Result);

            var third = simpleQueue.QueueAsync(async delegate
            {
                await Task.Delay(1000);

                return 3;
            });

            Assert.AreEqual(3, await third);
        }

        [TestMethod]
        public async Task TestSimpleAsyncQueueMerging()
        {
            var simpleQueue = new SimpleAsyncWorkerQueue<int>();

            var first = simpleQueue.QueueOrMergeAsync(1, async delegate
            {
                await Task.Delay(1000);

                return 1;
            });

            var second = simpleQueue.QueueOrMergeAsync(2, async delegate
            {
                await Task.Delay(1000);

                return 50;
            });

            var secondReplacement = simpleQueue.QueueOrMergeAsync(2, async delegate
            {
                await Task.Delay(1000);

                return 2;
            });

            await Task.Delay(1500);
            Assert.AreEqual(1, first.Result);

            await Task.Delay(500);
            Assert.IsFalse(second.IsCompleted);
            Assert.IsFalse(secondReplacement.IsCompleted);
            await Task.Delay(500);
            Assert.AreEqual(2, second.Result);
            Assert.AreEqual(2, secondReplacement.Result);

            var secondButNew = simpleQueue.QueueOrMergeAsync(2, async delegate
            {
                await Task.Delay(1000);

                return 22;
            });

            var secondButAlreadyRunning = simpleQueue.QueueOrMergeAsync(2, async delegate
            {
                await Task.Delay(1000);

                return 222;
            });

            Assert.AreEqual(22, await secondButNew);

            await Task.Delay(500);
            Assert.IsFalse(secondButAlreadyRunning.IsCompleted);
            await Task.Delay(500);
            Assert.AreEqual(222, secondButAlreadyRunning.Result);
        }
    }
}
