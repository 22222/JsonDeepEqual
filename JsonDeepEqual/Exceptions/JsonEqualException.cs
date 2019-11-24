using System;
using System.Collections.Generic;
using System.Text;

namespace Two.JsonDeepEqual.Exceptions
{
    /// <summary>
    /// Exception thrown when two JSON values are unexpectedly not equal.
    /// </summary>
    public class JsonEqualException : Exception
    {
        /// <summary>
        /// Constructs a default <see cref="JsonEqualException"/>.
        /// </summary>
        public JsonEqualException()
            : this(differences: null, message: null, innerException: null) { }

        /// <summary>
        /// Constructs a default <see cref="JsonEqualException"/> with a custom message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception, or null.</param>
        public JsonEqualException(string? message)
            : this(differences: null, message: message, innerException: null) { }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonEqualException"/> with a custom message
        /// and a references to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception, or null.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null.</param>
        public JsonEqualException(string? message, Exception? innerException)
            : this(differences: null, message: message, innerException: innerException) { }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonEqualException"/> with the specified differences.
        /// </summary>
        /// <param name="differences">The differences that caused this exception.</param>
        public JsonEqualException(IReadOnlyCollection<JsonDiffNode>? differences)
            : this(differences, message: null, innerException: null) { }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonEqualException"/> with the specified differences and a custom message.
        /// </summary>
        /// <param name="differences">The differences that caused this exception.</param>
        /// <param name="message">The error message that explains the reason for the exception, or null.</param>
        public JsonEqualException(IReadOnlyCollection<JsonDiffNode>? differences, string? message)
            : this(differences, message, innerException: null) { }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonEqualException"/> with the specified differences, a custom message,
        /// and a references to the inner exception that caused this exception.
        /// </summary>
        /// <param name="differences">The differences that caused this exception.</param>
        /// <param name="message">The error message that explains the reason for the exception, or null.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null.</param>
        public JsonEqualException(IReadOnlyCollection<JsonDiffNode>? differences, string? message, Exception? innerException)
            : base(message ?? "JsonAssert.Equal() Failure", innerException)
        {
            Differences = differences ?? Array.Empty<JsonDiffNode>();
        }

        /// <summary>
        /// The differences that caused this exception.
        /// </summary>
        public IReadOnlyCollection<JsonDiffNode> Differences { get; }

        /// <summary>
        /// The base message, without the description of the <see cref="Differences"/>.
        /// </summary>
        protected string BaseMessage => base.Message;

        /// <summary>
        /// A message the includes the base message and a description of the <see cref="Differences"/>.
        /// </summary>
        public override string Message
        {
            get
            {
                if (message == null)
                {
                    message = GenerateMessageFromDifferences(Differences, base.Message);
                }
                return message;
            }
        }

        private string? message;

        private static string GenerateMessageFromDifferences(IReadOnlyCollection<JsonDiffNode> differences, string? baseMessage)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(baseMessage))
            {
                sb.Append(baseMessage);
            }
            foreach (var difference in differences)
            {
                sb.AppendLine();
                sb.Append(difference.ToString());
            }
            return sb.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = GetType().ToString();

            var message = Message;
            if (!string.IsNullOrEmpty(message))
            {
                result += ": " + message;
            }

            var stackTrace = StackTrace;
            if (!string.IsNullOrEmpty(stackTrace))
            {
                result += Environment.NewLine + stackTrace;
            }
            return result;
        }
    }
}
