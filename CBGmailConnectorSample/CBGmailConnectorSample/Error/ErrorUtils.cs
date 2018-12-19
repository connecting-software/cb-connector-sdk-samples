using System.Diagnostics;
using Newtonsoft.Json;

namespace CBGmailConnectorSample.Error
{
    /// <summary>
    /// Class with utility methods to work with errors.
    /// </summary>
    public static class ErrorUtils
    {
        /// <summary>
        /// Extracts error details from an <see cref="ErrorDetails"/>.
        /// </summary>
        /// <param name="error">The ODataError instance to extract the error details from.</param>
        /// <param name="code">A data service-defined string which serves as a substatus to the HTTP response code.</param>
        /// <param name="message">A human readable message describing the error.</param>
        internal static void GetErrorDetails(RootError error, out string code, out string message)
        {
            Debug.Assert(error != null, "error != null");
            code = $"{error.Error?.Code}";
            message = error.Error?.Message ?? string.Empty;
        }
        /// <summary>
        /// Extracts error details from an <see cref="ErrorDetails"/>.
        /// </summary>
        /// <param name="content">The content to extract the error details from.</param>
        /// <param name="code">A data service-defined string which serves as a substatus to the HTTP response code.</param>
        /// <param name="message">A human readable message describing the error.</param>
        internal static void GetErrorDetails(string content, out string code, out string message)
        {
            try
            {
                var error = JsonConvert.DeserializeObject<RootError>(content);
                GetErrorDetails(error, out code, out message);
            }
            catch
            {
                code = string.Empty;
                message = content;
            }
        }
    }
}