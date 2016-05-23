using System;
using NUnit.Framework;
using Utilities;

namespace UtilitiesTests
{
    [TestFixture]
    public static class ThrowIfTests
    {
        [TestCase("foo")]
        [TestCase(5)]
        [TestCase(3.14)]
        [TestCase(true)]
        public static void ArgumentNull_ValidArgs_NoException(object arg)
        {
            Assert.DoesNotThrow(() =>
            {
                ThrowIf.ArgumentNull(arg, nameof(arg));
            }, "{0}.{1}() should not throw an exception if passed a non-null argument!",
                nameof(ThrowIf), nameof(ThrowIf.ArgumentNull));
        }
        
        [TestCase(null, "foo")]
        [TestCase(null, null)]
        public static void ArgumentNull_NullArg_ArgumentNullException(string arg, string nameOfArg)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ThrowIf.ArgumentNull(arg, nameOfArg);
            }, "{0}.{1}() should throw a {2} if passed a null argument!",
                nameof(ThrowIf), nameof(ThrowIf.ArgumentNull), nameof(ArgumentNullException));
        }

        [TestCase]
        public static void StringIsNullOrWhiteSpace_ValidArgs_NoException()
        {
            Assert.DoesNotThrow(() =>
            {
                ThrowIf.StringIsNullOrWhiteSpace("abcd1234", "foo");
            }, "{0}.{1}() should not throw an exception if passed an argument that isn't null or white space!",
                nameof(ThrowIf), nameof(ThrowIf.StringIsNullOrWhiteSpace));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        public static void StringIsNullOrWhiteSpace_NullOrWhiteSpaceArg_ArgumentNullException(string arg)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ThrowIf.StringIsNullOrWhiteSpace(arg, nameof(arg));
            }, "{0}.{1}() should throw a {2} if passed a null or white space argument!",
                nameof(ThrowIf), nameof(ThrowIf.StringIsNullOrWhiteSpace), nameof(ArgumentNullException));
        }
    }
}

