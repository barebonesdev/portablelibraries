using BareMvvm.Core.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolsPortable;

namespace BareMvvm.Core.Test
{
    [TestClass]
    public class TestBinding
    {
        public class MyTask : BindableBase
        {
            private string _name;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value, nameof(Name));
            }

            private MyClass _class;
            public MyClass Class
            {
                get => _class;
                set => SetProperty(ref _class, value, nameof(Class));
            }

            private double _percentComplete;
            public double PercentComplete
            {
                get => _percentComplete;
                set => SetProperty(ref _percentComplete, value, nameof(PercentComplete));
            }
        }

        public class MyClass : BindableBase
        {
            private string _name;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value, nameof(Name));
            }

            private byte[] _color;
            public byte[] Color
            {
                get => _color;
                set => SetProperty(ref _color, value, nameof(Color));
            }

            private MyTeacher _teacher;
            public MyTeacher Teacher
            {
                get => _teacher;
                set => SetProperty(ref _teacher, value, nameof(Teacher));
            }

            private int _priority;
            public int Priority
            {
                get => _priority;
                set => SetProperty(ref _priority, value, nameof(Priority));
            }
        }

        public class MyTeacher : BindableBase
        {
            private string _name;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value, nameof(Name));
            }
        }

        [TestMethod]
        public void TestOneLevelBinding()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int nameBindingExecutions = 0;
            int percentCompleteBindingExecutions = 0;

            bindingHost.SetBinding<string>(nameof(MyTask.Name), name =>
            {
                Assert.AreEqual(task.Name, name);
                nameBindingExecutions++;
            });

            bindingHost.SetBinding<double>(nameof(MyTask.PercentComplete), percentComplete =>
            {
                Assert.AreEqual(task.PercentComplete, percentComplete);
                percentCompleteBindingExecutions++;
            });

            task.Name = "Bookwork updated";
            task.PercentComplete = 0.6;

            Assert.AreEqual(2, nameBindingExecutions);
            Assert.AreEqual(2, percentCompleteBindingExecutions);

            bindingHost.Unregister();

            // Now any changes shouldn't trigger anything
            task.Name = "Bookwork 2";
            task.PercentComplete = 1;

            Assert.AreEqual(2, nameBindingExecutions);
            Assert.AreEqual(2, percentCompleteBindingExecutions);
        }

        [TestMethod]
        public void TestMultiLevelBinding()
        {
            var teacher1 = new MyTeacher()
            {
                Name = "Steven"
            };

            var teacher2 = new MyTeacher()
            {
                Name = "Stephanie"
            };

            var class1 = new MyClass()
            {
                Name = "Math",
                Color = new byte[] { 4, 3, 2 },
                Teacher = teacher1
            };

            var class2 = new MyClass()
            {
                Name = "Science",
                Color = new byte[] { 5, 6, 7 },
                Teacher = teacher2
            };

            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3,
                Class = class1
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int classNameExecutions = 0;
            int classTeacherExecutions = 0;
            int classTeacherNameExecutions = 0;

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                classNameExecutions++;
            });

            bindingHost.SetBinding<MyTeacher>("Class.Teacher", teacher =>
            {
                Assert.ReferenceEquals(task.Class.Teacher, teacher);
                classTeacherExecutions++;
            });

            bindingHost.SetBinding<string>("Class.Teacher.Name", teacherName =>
            {
                Assert.AreEqual(task.Class.Teacher.Name, teacherName);
                classTeacherNameExecutions++;
            });

            task.Class = class2; // This should trigger all bindings
            task.Class.Teacher = teacher1; // This should trigger only teacher bindings

            // These shouldn't trigger any of the bindings anymore since they're no longer referenced
            class1.Name = "Spanish";
            class1.Teacher = teacher2;
            teacher2.Name = "Bob";

            Assert.AreEqual(2, classNameExecutions);
            Assert.AreEqual(3, classTeacherExecutions);
            Assert.AreEqual(3, classTeacherNameExecutions);

            bindingHost.Unregister();

            // Now any of these changes shouldn't trigger anything
            task.Class = class1;

            Assert.AreEqual(2, classNameExecutions);
            Assert.AreEqual(3, classTeacherExecutions);
            Assert.AreEqual(3, classTeacherNameExecutions);
        }

        [TestMethod]
        public void TestBindingToNulls()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int classNameBindingExecutions = 0;
            int classPriorityBindingExecutions = 0;
            int classPriorityObjBindingExecutions = 0;

            // When property doesn't exist, should be default value of the type
            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class?.Name, className);
                classNameBindingExecutions++;
            });

            bindingHost.SetBinding<int>("Class.Priority", classPriority =>
            {
                Assert.AreEqual(task.Class?.Priority ?? default, classPriority);
                classPriorityBindingExecutions++;
            });

            // And with this binder, it should be null if not found
            bindingHost.SetBinding("Class.Priority", classPriorityObj =>
            {
                Assert.AreEqual(task.Class?.Priority, classPriorityObj);
                classPriorityObjBindingExecutions++;
            });

            // That should trigger another execution
            task.Class = new MyClass()
            {
                Name = "Spanish",
                Priority = 3
            };

            // And another execution (this time back to null)
            task.Class = null;

            Assert.AreEqual(3, classNameBindingExecutions);
            Assert.AreEqual(3, classPriorityBindingExecutions);
            Assert.AreEqual(3, classPriorityObjBindingExecutions);
        }

        [TestMethod]
        public void TestSettingDataContextLater()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3,
                Class = new MyClass()
                {
                    Name = "Math"
                }
            };

            BindingHost bindingHost = new BindingHost();

            int taskNameBindingExecutions = 0;
            int classNameBindingExecutions = 0;

            // Shouldn't execute until I set data context
            bindingHost.SetBinding<string>("Name", name =>
            {
                if (taskNameBindingExecutions == 0)
                {
                    Assert.AreEqual(task.Name, name);
                }
                else
                {
                    Assert.IsNull(name);
                }

                taskNameBindingExecutions++;
            });

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                if (classNameBindingExecutions == 0)
                {
                    Assert.AreEqual(task.Class.Name, className);
                }
                else
                {
                    Assert.IsNull(className);
                }

                classNameBindingExecutions++;
            });

            // This should cause them to execute
            bindingHost.DataContext = task;

            Assert.AreEqual(1, taskNameBindingExecutions);
            Assert.AreEqual(1, classNameBindingExecutions);

            // And then setting it to null should cause everything to execute again
            bindingHost.DataContext = null;

            Assert.AreEqual(2, taskNameBindingExecutions);
            Assert.AreEqual(2, classNameBindingExecutions);

            // And changing values shouldn't do anything
            task.Name = "Changed";
            task.Class.Name = "Changed";

            Assert.AreEqual(2, taskNameBindingExecutions);
            Assert.AreEqual(2, classNameBindingExecutions);
        }

        [TestMethod]
        public void TestChangingDataContext()
        {
            var class1 = new MyClass()
            {
                Name = "Math"
            };

            var class2 = new MyClass()
            {
                Name = "Science"
            };

            var task1 = new MyTask()
            {
                Name = "Bookwork",
                Class = class1
            };

            var task2 = new MyTask()
            {
                Name = "Essay",
                Class = class2
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task1
            };

            int nameExecutions = 0;
            int classNameExecutions = 0;

            bindingHost.SetBinding<string>("Name", name =>
            {
                if (nameExecutions == 0)
                {
                    Assert.AreEqual(task1.Name, name);
                }
                else
                {
                    Assert.AreEqual(task2.Name, name);
                }

                nameExecutions++;
            });

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                if (classNameExecutions == 0)
                {
                    Assert.AreEqual(task1.Class.Name, className);
                }
                else
                {
                    Assert.AreEqual(task2.Class.Name, className);
                }

                classNameExecutions++;
            });

            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(1, classNameExecutions);

            // This should re-trigger both bindings
            bindingHost.DataContext = task2;
            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(2, classNameExecutions);

            // Changing values in original task shouldn't trigger anything
            task1.Name = "Doesn't matter";
            class1.Name = "Class 1 doesn't matter";
            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(2, classNameExecutions);

            // Changing values in new task should trigger
            task2.Name = "Task 2 updated";
            task2.Class = class1;
            Assert.AreEqual(3, nameExecutions);
            Assert.AreEqual(3, classNameExecutions);

            // Changing value in old no longer referenced class shouldn't trigger
            class2.Name = "No longer referenced";
            Assert.AreEqual(3, nameExecutions);
            Assert.AreEqual(3, classNameExecutions);
        }

        [TestMethod]
        public void TestSettingValuesThroughBinding()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3,
                Class = new MyClass()
                {
                    Name = "Math"
                }
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int nameExecutions = 0;
            int classNameExecutions = 0;
            int nameAlwaysExecutions = 0;
            int classNameAlwaysExecutions = 0;

            bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                nameExecutions++;
            });

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                classNameExecutions++;
            });

            // These should always execute even when set through binding
            bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                nameAlwaysExecutions++;
            }, triggerEvenWhenSetThroughBinding: true);

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                classNameAlwaysExecutions++;
            }, triggerEvenWhenSetThroughBinding: true);

            // Should only have initial executions so far
            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(1, classNameExecutions);
            Assert.AreEqual(1, nameAlwaysExecutions);
            Assert.AreEqual(1, classNameAlwaysExecutions);

            bindingHost.SetValue("Name", "Bookwork updated");
            bindingHost.SetValue("Class.Name", "Math updated");

            // Even though values changed, bindings shouldn't re-execute since we set them ourselves
            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(1, classNameExecutions);

            // But the ones we said to update regardless should still update
            Assert.AreEqual(2, nameAlwaysExecutions);
            Assert.AreEqual(2, classNameAlwaysExecutions);

            // And setting programmatically should update all
            task.Name = "Bookwork 2";
            task.Class.Name = "Math 2";
            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(2, classNameExecutions);
            Assert.AreEqual(3, nameAlwaysExecutions);
            Assert.AreEqual(3, classNameAlwaysExecutions);

            // Setting class through binding should cause the class name binding to update
            bindingHost.SetValue("Class", new MyClass()
            {
                Name = "Spanish"
            });

            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(3, classNameExecutions);
            Assert.AreEqual(3, nameAlwaysExecutions);
            Assert.AreEqual(4, classNameAlwaysExecutions);
        }

        [TestMethod]
        public void TestBindingToDataContext()
        {
            var task1 = new MyTask()
            {
                Name = "Bookwork 1"
            };

            var task2 = new MyTask()
            {
                Name = "Bookwork 2"
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task1
            };

            int timesInvoked = 0;

            bindingHost.SetBinding<MyTask>("", task =>
            {
                if (timesInvoked == 0)
                {
                    Assert.ReferenceEquals(task1, task);
                }
                else
                {
                    Assert.ReferenceEquals(task2, task);
                }

                timesInvoked++;
            });

            Assert.AreEqual(1, timesInvoked);

            bindingHost.DataContext = task2;

            Assert.AreEqual(2, timesInvoked);
        }
    }
}
