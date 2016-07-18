using System;
using NUnit.Framework;
using NTestController;
using Utilities;
using Logger;
using System.Collections.Generic;

namespace NTestControllerTests
{
    [TestFixture]
    public class TestQueueTests
    {
        private TestQueue _testqueue = null;

        [SetUp]
        public void Setup()
        {
            _testqueue = new TestQueue();
        }
        
        [TestCase]
        public void EnqueueTestToRun_NullTest_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _testqueue.EnqueueTestToRun(null),
                "{0}.{1}() should throw an ArgumentNullException if you pass it a null test!",
                nameof(TestQueue), nameof(TestQueue.EnqueueTestToRun));
        }

        [Test, Combinatorial]
        public void EnqueueTestToRun_ValidTest_TestCanBeDequeued(
            [Values(null, "MyNamespace")]   string testNamespace,
            [Values(null, "MyClass")]       string testClass,
            [Values(null, "MyFunction")]    string testFunction,
            [Values(null, "A.B.C(1,'a')")]  string testName
        )
        {
            // Setup:
            Test test = new Test {
                TestNamespace = testNamespace,
                TestClass = testClass,
                TestFunction = testFunction,
                TestName = testName ?? StringUtils.FormatInvariant("{0}.{1}.{2}",
                    testNamespace, testClass, testFunction)
            };

            // Execute:
            Assert.DoesNotThrow(() => _testqueue.EnqueueTestToRun(test),
                "{0}.{1}() shouldn't throw an Exception if you pass it a valid test!",
                nameof(TestQueue), nameof(TestQueue.EnqueueTestToRun));

            // Verify:
            Test dequeuedTest = null;

            Assert.DoesNotThrow(() => dequeuedTest = _testqueue.DequeueTestToRun(),
                "{0}.{1}() shouldn't throw an exception if a test has been added to the queue!",
                nameof(TestQueue), nameof(TestQueue.EnqueueTestToRun));

            AssertTestsAreEqual(test, dequeuedTest);
        }

        [TestCase]
        public void DequeueTestToRun_NoTests_ReturnsNull()
        {
            Test dequeuedTest = null;

            // Execute:
            Assert.DoesNotThrow(() => dequeuedTest = _testqueue.DequeueTestToRun(),
                "{0}.{1}() shouldn't throw an exception!",
                nameof(TestQueue), nameof(TestQueue.DequeueTestToRun));

            // Verify:
            Assert.IsNull(dequeuedTest,
                "{0}.{1}() should return null if the queue is empty!",
                nameof(TestQueue), nameof(TestQueue.DequeueTestToRun));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void DequeueTestToRun_MultipleValidTests_AllTestsCanBeDequeued(int numberOfTests)
        {
            // Setup:
            IList<Test> tests = new List<Test>();

            for (int i = 0; i < numberOfTests; ++i)
            {
                Test test = new Test {
                    TestName = StringUtils.FormatInvariant("Namespace{0}.Class{0}.Function{0}", i)
                };

                tests.Add(test);

                Assert.DoesNotThrow(() => _testqueue.EnqueueTestToRun(test),
                    "{0}.{1}() shouldn't throw an Exception if you pass it a valid test!",
                    nameof(TestQueue), nameof(TestQueue.EnqueueTestToRun));
            }

            // Execute & Verify:
            int numberOfTestsDequeued = 0;
            Test dequeuedTest = null;

            Assert.DoesNotThrow(() => dequeuedTest = _testqueue.DequeueTestToRun(),
                "{0}.{1}() shouldn't throw an exception if a test has been added to the queue!",
                nameof(TestQueue), nameof(TestQueue.EnqueueTestToRun));
            
            do
            {
                if (dequeuedTest != null)
                {
                    ++numberOfTestsDequeued;

                    Assert.That(tests.Contains(dequeuedTest),
                        "Dequeued test '{0}' wasn't found in the list of tests that was enqueued!",
                        dequeuedTest.TestName);

                    Assert.DoesNotThrow(() => dequeuedTest = _testqueue.DequeueTestToRun(),
                        "{0}.{1}() shouldn't throw an exception if a test has been added to the queue!",
                        nameof(TestQueue), nameof(TestQueue.EnqueueTestToRun));
                }
            } while (dequeuedTest != null);

            Assert.AreEqual(numberOfTests, numberOfTestsDequeued,
                "The number of tests Enqueued: {0} doesn't equal the number Dequeued: {1}!",
                numberOfTests, numberOfTestsDequeued);
        }

        [TestCase]
        public void AddCompletedTest_NullTest_NullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() => _testqueue.AddCompletedTest(null),
                "{0}.{1}() should throw an ArgumentNullException if you pass it a null test!",
                nameof(TestQueue), nameof(TestQueue.AddCompletedTest));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddCompletedTest_ValidTests_HasCompletedTests(int numberOfTests)
        {
            // Setup:
            IList<Test> tests = new List<Test>();

            for (int i = 0; i < numberOfTests; ++i)
            {
                Test test = new Test {
                    TestName = StringUtils.FormatInvariant("Namespace{0}.Class{0}.Function{0}", i)
                };

                tests.Add(test);

                // Execute:
                Assert.DoesNotThrow(() => _testqueue.AddCompletedTest(test),
                    "{0}.{1}() shouldn't throw an exception when adding a valid test!",
                    nameof(TestQueue), nameof(TestQueue.AddCompletedTest));
            }

            // Verify:
            Assert.AreEqual(tests.Count, _testqueue.CompletedTests.Count,
                "{0}.{1} should equal the number of tests added (i.e. '{2}')!",
                nameof(TestQueue), nameof(TestQueue.CompletedTests), tests.Count);

            foreach (Test test in _testqueue.CompletedTests)
            {
                Assert.That(tests.Contains(test),
                    "Test '{0}' was not found in {1}.{2}!",
                    test.TestName,nameof(TestQueue), nameof(TestQueue.CompletedTests));
            }
        }


        /// <summary>
        /// Asserts the two Tests are equal.
        /// </summary>
        /// <param name="test1">First test to compare.</param>
        /// <param name="test2">Second test to compare.</param>
        private void AssertTestsAreEqual(Test test1, Test test2)
        {
            Assert.AreEqual(test1.TestNamespace, test2.TestNamespace,
                "TestNamespace of test1 doesn't match test2!");
            Assert.AreEqual(test1.TestClass, test2.TestClass,
                "TestClass of test1 doesn't match test2!");
            Assert.AreEqual(test1.TestFunction, test2.TestFunction,
                "TestFunction of test1 doesn't match test2!");
            Assert.AreEqual(test1.TestName, test2.TestName,
                "TestName of test1 doesn't match test2!");
            Assert.AreEqual(test1.TestRuns.Count, test2.TestRuns.Count,
                "TestRuns of test1 has {0} runs, but test2 has {1} runs!",
                test1.TestRuns.Count, test2.TestRuns.Count);
            Assert.AreEqual(test1.ExtendedProperties.Count, test2.ExtendedProperties.Count,
                "ExtendedProperties of test1 has {0} properties, but test2 has {1} properties!",
                test1.ExtendedProperties.Count, test2.ExtendedProperties.Count);
        }
    }
}

