using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolsPortable.Locks;
using System.Threading.Tasks;

namespace ToolsPortable.Test
{
    [TestClass]
    public class TestMyAsyncLock
    {
        private MyAsyncLock Lock = new MyAsyncLock();

        [TestMethod]
        public async Task Test01()
        {
            bool completed = false;

            var dontWait = Task.Run(async delegate
            {
                using (await Lock.LockAsync())
                {
                    await Task.Delay(2000);
                    completed = true;
                }
            });

            await Task.Delay(500);

            using (await Lock.LockAsync())
            {
                Assert.IsTrue(completed, "completed was false, indicating that the lock did not wait till the previous lock completed");
            }
        }

        [TestMethod]
        public async Task Test02()
        {
            bool completed = false;

            var dontWait = Task.Run(async delegate
            {
                using (await Lock.LockAsync())
                {
                    await Task.Delay(2000);
                    completed = true;
                }
            });

            await Task.Delay(500);

            await Task.Run(async delegate
            {
                using (await Lock.LockAsync())
                {
                    Assert.IsTrue(completed, "completed was false, indicating that the lock did not wait till the previous lock completed");
                }
            });
        }

        [TestMethod]
        public async Task Test03()
        {
            bool completed = false;

            var first = LockAndWork(null, delegate { completed = true; });
            var second = LockAndWork(delegate
            {
                if (!completed)
                {
                    Assert.Fail("First hasn't completed");
                }
            }, null);

            await first;
            Assert.IsFalse(second.IsCompleted);
            await second;
        }

        private async Task LockAndWork(Action onStarted, Action onCompleted)
        {
            using (await Lock.LockAsync())
            {
                onStarted?.Invoke();
                await Task.Delay(500);
                onCompleted?.Invoke();
            }
        }

        [TestMethod]
        public async Task Test04()
        {
            int state = 0;

            var first = LockAndWork(delegate { Assert.AreEqual(0, state); state = 1; }, delegate { Assert.AreEqual(1, state); state = 1; });
            var second = LockAndWork(delegate { Assert.AreEqual(1, state); state = 2; }, delegate { Assert.AreEqual(2, state); state = 2; });
            var third = LockAndWork(delegate { Assert.AreEqual(2, state); state = 3; }, delegate { Assert.AreEqual(3, state); state = 3; });
            var fourth = LockAndWork(delegate { Assert.AreEqual(3, state); state = 4; }, delegate { Assert.AreEqual(4, state); state = 4; });

            await fourth;
            Assert.IsTrue(first.IsCompleted);
            Assert.IsTrue(second.IsCompleted);
            Assert.IsTrue(third.IsCompleted);
            Assert.IsTrue(fourth.IsCompleted);
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task TestTimeouts01()
        {
            var first = LockAndWork(null, null);
            using (await Lock.LockAsync(100))
            {
                Assert.Fail("Lock shouldn't have been established, since timeout should occur");
            }
        }

        [TestMethod]
        public async Task TestTimeouts02()
        {
            bool completed = false;
            var first = LockAndWork(null, delegate { completed = true; });
            using (await Lock.LockAsync(700))
            {
                Assert.IsTrue(completed);
            }
        }

        [TestMethod]
        public async Task TestTimeouts03()
        {
            // The middle lock should timeout, but the last one should still execute

            var first = LockWithTimeout(500);
            var second = LockWithTimeout(10);
            var third = LockWithTimeout(800);

            await first;

            // Second one should have thrown exception
            Assert.IsTrue(second.IsFaulted);

            // Third one shouldn't have completed yet
            Assert.IsFalse(third.IsCompleted);

            await third;
        }

        private async Task LockWithTimeout(int milliseconds)
        {
            using (await Lock.LockAsync(milliseconds))
            {
                await Task.Delay(500);
            }
        }
    }
}
