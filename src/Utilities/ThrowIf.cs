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
        /// Throws an ArgumentNullException if the argument is null.
        /// Example:  ThrowIf.ArgumentNull(car, nameof(car));
        /// </summary>
        /// <param name="arg">The argument to check for null.</param>
        /// <param name="nameOfArg">The name of the argument (use the nameof() function for this).</param>
        /// <param name="message">An additional message to include in the exception.</param>
        public static void ArgumentNull([ValidatedNotNull] object arg, string nameOfArg, string message)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameOfArg, message);
            }
        }
    }
}
