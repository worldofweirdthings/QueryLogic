using System;
using System.Runtime.Serialization;

namespace QueryLogic
{
    /// <summary>
    /// Represents an error which occurred during the execution of a SQL command
    /// </summary>
    [Serializable]
    public class QueryException : Exception
    {
        /// <summary>
        /// Creates new instance of the exception
        /// </summary>
        /// <param name="message">Error message</param>
        public QueryException(string message) : base(message) { }

        /// <summary>
        /// Creates new instance of the exception
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Exception captured and encapsulated for review</param>
        public QueryException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Creates new instance of the exception
        /// </summary>
        /// <param name="info">Serialization metadata</param>
        /// <param name="context">Serialization logic metadata</param>
        public QueryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}