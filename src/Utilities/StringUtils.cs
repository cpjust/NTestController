using System;
using System.Globalization;

namespace Utilities
{
    public static class StringUtils
    {
        /// <summary>
        /// Formats the string using invariant culture info.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatInvariant(this string format, params Object[] args)
        {
            ThrowIf.StringIsNullOrWhiteSpace(format, nameof(format));
            ThrowIf.ArgumentNull(args, nameof(args));

            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Returns true if this string starts with the search value.
        /// </summary>
        /// <param name="thisString">The string to search.</param>
        /// <param name="searchValue">The value to search for at the beginning of the string.</param>
        /// <param name="ignoreCase">(optional) Pass true to make the search case insensitive.</param>
        /// <returns>True if the string starts with the search value, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]    // No!  That's stupid!
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]  // This is a stupid rule.
        public static bool StartsWithInvariant(this string thisString, string searchValue, bool ignoreCase = false)
        {
            ThrowIf.StringIsNullOrWhiteSpace(thisString, nameof(thisString));
            ThrowIf.StringIsNullOrWhiteSpace(searchValue, nameof(searchValue));

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return thisString.StartsWith(searchValue, comparison);
        }
    }
}

