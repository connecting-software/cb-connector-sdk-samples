using System;
using System.Linq;
using CBGmailConnectorSample.Metadata;
using CBGmailConnectorSample.Result.Json;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;

namespace CBGmailConnectorSample.Result
{
    /// <summary> Component that is used to fill the <see cref="ITableResultLoader"/>. </summary>
    public class TableResultLoader
    {
        //The loader used to load a valid result expected by the CB Server.
        private readonly ITableResultLoader _loader;

        public TableResultLoader(ITableResultLoader loader)
        {
            _loader = loader;
        }
        
        /// <summary> Called when the JSON data has been serialized. </summary>
        /// <param name="source">The source.</param>
        /// <param name="args">The <see cref="JsonEventArgs"/> instance containing the event data.</param>
        public void OnJsonRead(object source, JsonEventArgs args)
        {
            foreach (var row in args.Rows)
            {
                if (row.Columns.Count == 0)
                {
                    _loader.Add(_loader.NewRow());
                    break;
                }
                var resultRow = _loader.NewRow();
                var buffer = new object[resultRow.Columns.Count];
                foreach (var column in row.Columns)
                {
                    var fullColumnName = column.Key;
                    var columnName = fullColumnName.TrimEnd('/');
                    var simpleColumnName = fullColumnName.Substring(0, columnName.LastIndexOf("/", StringComparison.Ordinal));

                    var argument = resultRow.Columns.Select(m => m.MetadataColumn).OfType<Column>().SingleOrDefault(c => c.Path == fullColumnName);
                    if (argument == null) continue;

                    var index = resultRow.Columns.Select(m => m.MetadataColumn).OfType<Column>().ToList().IndexOf(argument);
                    var values = row[simpleColumnName];
                    var value = column.Value < values.Count ? values[column.Value] : null;
                    buffer[index] = value;
                }

                resultRow.SetValues(buffer);
                _loader.Add(resultRow);
            }
        }
    }
}