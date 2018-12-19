using System.Collections.Generic;

namespace CBGmailConnectorSample.Error
{
    public class RootError
    {
        public Error Error { get; set; }
    }


    /// <summary> Represents an JSON error. </summary>
    public class Error
    {
        /// <summary>Gets or sets the error details.</summary>
        /// <returns>The error details.</returns>
        public IList<ErrorDetails> Errors { get; set; } 

        /// <summary>Gets or sets the error code.</summary>
        /// <returns>The error code.</returns>
        public int Code { get; set; }

        /// <summary>Gets or sets the error message.</summary>
        /// <returns>The error message.</returns>
        public string Message { get; set; }
    }

    /// <summary> Represents an JSON error details. </summary>
    public class ErrorDetails
    {
        public string Domain { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
    }
}