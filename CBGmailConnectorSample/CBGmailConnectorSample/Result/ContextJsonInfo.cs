using System.Collections.Generic;

namespace CBGmailConnectorSample.Result
{
    /// <summary> Class representing required information for context JSON Reader. </summary>
    public class ContextJsonInfo
    {
        /// <summary> Gets a list of JSON property names.</summary>
        public List<string> PropertyNameList { get; } = new List<string>();
    }
}