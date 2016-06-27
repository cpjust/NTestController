using NUnit.Framework;
using System;
using Utilities;

namespace UtilitiesTests
{
    [TestFixture]
    public static class StringUtilsTests
    {
        [TestCase("Testing 1, 2, 3", "Testing {0}, {1}, {2}", 1, 2, 3)]
        [TestCase("Mixed Types: 1, Two, 3.14, True", "Mixed Types: {0}, {1}, {2}, {3}", 1, "Two", 3.14, true)]
        public static void FormatInvariant_ValidArgs_ProducesCorrectStrings(string expectedValue, string format, params Object[] args)
        {
            string result = StringUtils.FormatInvariant(format, args);

            Assert.AreEqual(expectedValue, result, "format string: '{0}' with args: '{1}' should produce: '{2}', but it returned: '{3}'",
                format, string.Join(", ", args), expectedValue, result);
        }

        [TestCase("{0} {1}", null)]
        [TestCase(null, new Object[] {1, 2})]
        [TestCase("", new Object[] {1, 2})]
        [TestCase(" ", new Object[] {1, 2})]
        [TestCase("\t", new Object[] {1, 2})]
        [TestCase("\n", new Object[] {1, 2})]
        public static void FormatInvariant_NullOrWhiteSpaceArgs_ArgumentNullException(string format, Object[] args)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                StringUtils.FormatInvariant(format, args);
            }, "{0}.{1}() should throw a {2} when passed in a null argument!",
                nameof(StringUtils), nameof(StringUtils.FormatInvariant), nameof(ArgumentNullException));
        }
    }
}

