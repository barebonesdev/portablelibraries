using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolsPortable.Locks;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ToolsPortable.Test
{
    [TestClass]
    public class TestMyAsyncReadWriteLock
    {
        private MyAsyncReadWriteLock Lock = new MyAsyncReadWriteLock();

        [TestMethod]
        public async Task Test01()
        {
            bool completed = false;

            var dontWait = Task.Run(async delegate
            {
                using (await Lock.LockWriteAsync())
                {
                    await Task.Delay(2000);
                    completed = true;
                }
            });

            await Task.Delay(500);

            using (await Lock.LockWriteAsync())
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
                using (await Lock.LockWriteAsync())
                {
                    await Task.Delay(2000);
                    completed = true;
                }
            });

            await Task.Delay(500);

            await Task.Run(async delegate
            {
                using (await Lock.LockWriteAsync())
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
            using (await Lock.LockWriteAsync())
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
            using (await Lock.LockWriteAsync(100))
            {
                Assert.Fail("Lock shouldn't have been established, since timeout should occur");
            }
        }

        [TestMethod]
        public async Task TestTimeouts02()
        {
            bool completed = false;
            var first = LockAndWork(null, delegate { completed = true; });
            using (await Lock.LockWriteAsync(700))
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
            using (await Lock.LockWriteAsync(milliseconds))
            {
                await Task.Delay(500);
            }
        }





        [TestMethod]
        public async Task TestNonConcurrency01()
        {
            // This makes sure that locks are exited and can re-enter later
            // Just super fundamental stuff

            using (await Lock.LockReadAsync())
            {
                await Task.Delay(50);
            }

            using (await Lock.LockReadAsync())
            {
                await Task.Delay(50);
            }

            await Task.Delay(50);

            using (await Lock.LockReadAsync())
            {

            }

            using (await Lock.LockReadAsync())
            {

            }

            using (await Lock.LockWriteAsync())
            {
                await Task.Delay(50);
            }

            using (await Lock.LockReadAsync())
            {

            }
        }

        [TestMethod]
        public async Task TestMultipleReads01()
        {
            bool completed = false;
            using (await Lock.LockReadAsync())
            {
                using (await Lock.LockReadAsync())
                {
                    await Task.Run(async delegate
                    {
                        using (await Lock.LockReadAsync())
                        {
                            await Task.Delay(50);
                            completed = true;
                        }
                    });
                }
            }

            Assert.IsTrue(completed);
        }

        [TestMethod]
        public async Task TestReadAndWrite01()
        {
            bool readCompleted = false;
            bool writeCompleted = false;

            var readTask = Read(500, null, delegate { readCompleted = true; });
            var writeTask = Write(500, delegate { Assert.IsTrue(readCompleted); }, delegate { writeCompleted = true; });

            await readTask;
            Assert.IsFalse(writeTask.IsCompleted);

            await writeTask;
            Assert.IsTrue(writeCompleted);
        }

        [TestMethod]
        public async Task TestReadAndWrite02()
        {
            bool readCompleted = false;
            bool read2Completed = false;
            bool writeCompleted = false;

            var readTask = Read(500, null, delegate { readCompleted = true; });
            var readTask2 = Read(700, null, delegate { read2Completed = true; });
            var writeTask = Write(500, delegate { Assert.IsTrue(readCompleted); Assert.IsTrue(read2Completed); }, delegate { writeCompleted = true; });

            await readTask;
            await readTask2;
            Assert.IsFalse(writeTask.IsCompleted);

            await writeTask;
            Assert.IsTrue(writeCompleted);
        }

        [TestMethod]
        public async Task TestReadAndWriteWithTimeout01()
        {
            bool readCompleted = false;
            bool writeCompleted = false;

            var readTask = Read(500, null, delegate { readCompleted = true; }, 500);
            var writeTask = Write(500, delegate { Assert.Fail("I shouldn't have started"); }, delegate { Assert.Fail("I shouldn't be hit"); }, 200);
            var write2Task = Write(500, delegate { Assert.IsTrue(readCompleted); }, delegate { writeCompleted = true; }, 800);

            try
            {
                await writeTask;
            }
            catch (TimeoutException)
            {
                Assert.IsFalse(readCompleted);
            }

            await write2Task;
            Assert.IsTrue(writeCompleted);
        }

        private async Task Read(int duration, Action onStarted, Action onCompleted, int millisecondsTimeout = int.MaxValue)
        {
            using (await Lock.LockReadAsync(millisecondsTimeout))
            {
                onStarted?.Invoke();
                await Task.Delay(duration);
                onCompleted?.Invoke();
            }
        }

        private async Task Write(int duration, Action onStarted, Action onCompleted, int millisecondsTimeout = int.MaxValue)
        {
            using (await Lock.LockWriteAsync(millisecondsTimeout))
            {
                onStarted?.Invoke();
                await Task.Delay(duration);
                onCompleted?.Invoke();
            }
        }

        [TestMethod]
        public async Task TestWriteAndRead01()
        {
            bool readCompleted = false;
            bool read2Completed = false;
            bool writeCompleted = false;

            var writeTask = Write(500, null, delegate { writeCompleted = true; });
            var readTask = Read(500, delegate { Assert.IsTrue(writeCompleted); }, delegate { readCompleted = true; });
            var readTask2 = Read(700, delegate { Assert.IsTrue(writeCompleted); }, delegate { read2Completed = true; });

            await writeTask;
            Assert.IsFalse(readTask.IsCompleted);
            Assert.IsFalse(readTask2.IsCompleted);

            await readTask;
            await readTask2;

            Assert.IsTrue(readCompleted);
            Assert.IsTrue(read2Completed);
        }

        [TestMethod]
        public async Task TestWriteAndReadWithTimeout01()
        {
            bool read2Completed = false;
            bool writeCompleted = false;
            bool write2Completed = false;

            var writeTask = Write(500, null, delegate { writeCompleted = true; }, 800);
            var readTask = Read(500, delegate { Assert.Fail("Shouldn't hit"); }, delegate { Assert.Fail("Shouldn't hit"); }, 200);
            var readTask2 = Read(700, delegate { Assert.IsTrue(writeCompleted); }, delegate { read2Completed = true; }, 800);
            var writeTask2 = Write(500, delegate { Assert.IsTrue(read2Completed); }, delegate { write2Completed = true; }, 3000);

            await writeTask;
            Assert.IsTrue(readTask.IsFaulted);
            Assert.IsFalse(readTask2.IsCompleted);

            await readTask2;

            Assert.IsTrue(read2Completed);
            Assert.IsFalse(writeTask2.IsCompleted);

            await writeTask2;

            Assert.IsTrue(write2Completed);
        }

        [TestMethod]
        public async Task TestExceptions01()
        {
            try
            {
                using (await Lock.LockWriteAsync())
                {
                    await Task.Delay(50);
                    throw new Exception("Uh ohs");
                }
            }
            catch
            {

            }

            // Lock should still be able to enter
            using (await Lock.LockWriteAsync())
            {

            }
        }

        [TestMethod]
        public async Task TestExceptions02()
        {
            // This tests scenario where WriteLock is waiting on another WriteLock, and the first
            // WriteLock throws an exception (second WriteLock should be able to continue then)
            bool throwingException = false;

            var firstTask = Task.Run(async delegate
            {
                try
                {
                    using (await Lock.LockWriteAsync())
                    {
                        await Task.Delay(1000);
                        throwingException = true;
                        throw new Exception("Uh ohs");
                    }
                }
                catch
                {

                }
            });

            await Task.Delay(50);
            Assert.IsFalse(throwingException);

            // Lock should still be able to enter
            using (await Lock.LockWriteAsync())
            {
                Assert.IsTrue(throwingException);
            }
        }

        [TestMethod]
        public async Task TestFIFO01()
        {
            bool firstCompleted = false;
            bool secondCompleted = false;
            bool thirdCompleted = false;

            var first = Read(500, null, delegate
            {
                firstCompleted = true;
            });

            var second = Write(500, delegate
            {
                Assert.IsTrue(firstCompleted);
                Assert.IsFalse(thirdCompleted);
            }, delegate
            {
                secondCompleted = true;
            });

            var third = Read(500, delegate
            {
                Assert.IsTrue(secondCompleted);
            }, delegate
            {
                thirdCompleted = true;
            });

            await third;
        }

        [TestMethod]
        public async Task TestMergingReads01()
        {
            bool firstCompleted = false;
            bool secondCompleted = false;

            var first = Read(1000, null, delegate
            {
                Assert.IsTrue(secondCompleted);
                firstCompleted = true;
            });

            var second = Read(500, null, delegate
            {
                secondCompleted = true;
            });

            var third = Write(500, null, delegate
            {
                Assert.IsTrue(firstCompleted);
                Assert.IsTrue(secondCompleted);
            });

            await first;
            await second;
            await third;
        }

        [TestMethod]
        public async Task TestMergingReads02()
        {
            // Tests merging reads all at the front

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Read(500, null, null));
            }

            // Make sure they all finished sequentually
            await Task.Delay(700);

            foreach (var t in tasks)
            {
                Assert.IsTrue(t.IsCompleted);
            }
        }

        [TestMethod]
        public async Task TestMergingReads03()
        {
            // Tests merging reads after an active write lock

            List<Task> tasks = new List<Task>();
            bool hasWriteFinished = false;

            // Start a write task so there's something before
            var write = Write(500, null, delegate
            {
                hasWriteFinished = true;
            });

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Read(500, delegate
                {
                    Assert.IsTrue(hasWriteFinished);
                }, null));
            }

            var lastWrite = Write(500, null, null);

            // Make sure they all finished sequentually
            await Task.Delay(1200);

            foreach (var t in tasks)
            {
                Assert.IsTrue(t.IsCompleted);
            }

            await Task.Delay(500);

            Assert.IsTrue(lastWrite.IsCompleted);
            await lastWrite;
        }
    }
}
