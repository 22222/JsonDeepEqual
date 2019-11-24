using System;

namespace Two.JsonDeepEqual.Exceptions
{
    /// <summary>
    /// Exception thrown when two JSON values are unexpectedly equal.
    /// </summary>
    public class JsonNotEqualException : Exception
    {
        /// <summary>
        /// Constructs a default <see cref="JsonNotEqualException"/>.
        /// </summary>
        public JsonNotEqualException()
            : this(message: null, innerException: null) { }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonNotEqualException"/> with a custom message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception, or null.</param>
        public JsonNotEqualException(string? message)
            : this(message, innerException: null) { }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonNotEqualException"/> with a custom message
        /// and a references to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception, or null.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null.</param>
        public JsonNotEqualException(string? message, Exception? innerException)
            : base(message ?? "JsonDeepEqualAssert.NotEqual() Failure", innerException) { }
    }
}
