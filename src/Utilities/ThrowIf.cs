using System;

[assembly: CLSCompliant(true)]
namespace Utilities
{
    // The naming is important to inform FxCop.
    sealed class ValidatedNotNullAttribute : Attribute { }

    public static class ThrowIf
    {
        /// <summary>
        /// Throws an ArgumentNullException if the argument is null.
        /// Example:  ThrowIf.ArgumentNull(car, nameof(car));
        /// </summary>
        /// <param name="arg">The argument to check for null.</param>
        /// <param name="nameOfArg">The name of the argument (use the nameof() function for this).</param>
        public static void ArgumentNull([ValidatedNotNull] object arg, string nameOfArg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameOfArg);
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the string is null or white space.
        /// Example:  ThrowIf.StringIsNullOrWhiteSpace(name, nameof(name));
        /// </summary>
        /// <param name="arg">The string to check for null or white space.</param>
        /// <param name="nameOfArg">Name of string variable.</param>
        public static void StringIsNullOrWhiteSpace([ValidatedNotNull] string arg, string nameOfArg)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                throw new ArgumentNullException(nameOfArg);
            }
        }
    }
}
