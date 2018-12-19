using System.Collections.Generic;

namespace CBGmailConnectorSample.Result.Json
{
    /// <summary>
    /// Provides a base class for building the Json Path.
    /// </summary>
    public class JsonPathBuilder
    {
        private readonly HashSet<string> _strings = new HashSet<string>();

        /// <summary> The path delimiter. </summary>
        public readonly string PathDelimiter = "/";

        /// <summary> Inserts a specified string. </summary>
        /// <param name="str">A string.</param>
        /// <returns>The string</returns>
        public string Insert(string str)
        {
            if (!_strings.Contains(str)) _strings.Add(str);
            return str;
        }
    }
}