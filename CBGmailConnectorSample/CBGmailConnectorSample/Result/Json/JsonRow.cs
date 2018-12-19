using System;
using System.Collections.Generic;
using System.Linq;

namespace CBGmailConnectorSample.Result.Json
{
    /// <summary> Class representing a row of data as a dictionary. </summary>
    public class JsonRow : Dictionary<string, List<object>>
    {
        internal JsonRow(IDictionary<string, int> columns)
        {
            Columns = columns;
        }

        internal IDictionary<string, int> Columns { get; }

        /// <summary> Gets or sets the value associated with the specified key. </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="Exception">Fatal error</exception>
        public object this[JsonPath key]
        {
            get
            {
                var parentPathName = key.ParentPathName();
                if (TryGetValue(parentPathName, out var values))
                {
                    var path = key.PathName();
                    if (Columns.TryGetValue(path, out var index))
                        if (index < values.Count)
                            return values[index];
                }

                return null;
            }
            set
            {
                var parentStr = key.ParentPathName();
                if (!TryGetValue(parentStr, out var values))
                    throw new Exception($"{GetType().Name}: Fatal error.");
                var keyStr = key.PathName();
                int index;
                if (!Columns.ContainsKey(keyStr))
                {
                    index = Columns.Keys.Count(_ =>
                        _.StartsWith(parentStr) && _.Length > parentStr.Length &&
                        _.IndexOf('/', parentStr.Length + 1) == -1);
                    Columns.Add(keyStr, index);
                }
                else
                {
                    index = Columns[keyStr];
                }

                for (var i = values.Count; i <= index; i++) values.Add(null);
                values[index] = value;
            }
        }

        /// <summary> Adds the specified key and value to the dictionary. </summary>
        /// <param name="name">The key of the element to add.</param>
        /// <param name="values">The value of the element to add.</param>
        public void AddValues(string name, List<object> values)
        {
            if (ContainsKey(name)) return;
            var buffer = values ?? new List<object>();
            Add(name, buffer);
        }
    }
}