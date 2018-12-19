using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CBGmailConnectorSample.Metadata;
using MG.CB.Command.DataHandler.Argument.Interfaces;

namespace CBGmailConnectorSample.Command
{
    public static class ExtensionMethods
    {
        public static void AddRange(this List<string> _this, ICollection<IColumnArgument> arguments)
        {
            foreach (var argument in arguments)
            {
                var columnMetadata = argument.MetadataColumn as Column;
                Debug.Assert(columnMetadata != null, "columnMetadata != null");

                var path = columnMetadata.Path;
                var entries = path.TrimStart('/').Split('/');

                foreach (var entry in entries.Take(entries.Length - 1))
                {
                    if(_this.Contains(entry)) continue;
                    _this.Add(entry);
                }
            }
        }
    }
}