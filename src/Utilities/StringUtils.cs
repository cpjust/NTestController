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
    }
}

