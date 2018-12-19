using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CBGmailConnectorSample.Result.Json
{
    /// <summary> Represents the base class for classes that contains event data. </summary>
    /// <seealso cref="EventArgs" />
    public class JsonEventArgs : EventArgs
    {
        /// <summary> Gets or sets the JSON rows. </summary>
        public IList<JsonRow> Rows { get; set; }
    }

    /// <summary>
    /// Represents a reader that provides access to serialized JSON data.
    /// </summary>
    public class JsonResultReader
    {
        #region Fields

        private readonly IDictionary<string, int> _columns = new Dictionary<string, int>();
        private readonly IList<JsonRow> _rows = new List<JsonRow>();
        private bool _copyRows;
        private JsonRow _currentRow;
        private JsonPathElement _previousElement;

        #endregion

        /// <summary> Occurs when the JSON data has been serialized. </summary>
        public event EventHandler<JsonEventArgs> JsonRead;

        /// <summary> Called when the JSON data has been serialized. </summary>
        protected virtual void OnJsonRead()
        {
            JsonRead?.Invoke(this, new JsonEventArgs {Rows = _rows});
        }

        /// <summary> Reads JSON tokens from the stream. </summary>
        public virtual void Read(Stream stream, ContextJsonInfo context)
        {
            Debug.Assert(stream != null, "stream != null");
            using (var streamReader = new StreamReader(stream))
            {
                using (var reader = new JsonTextReader(streamReader))
                {
                    var path = new JsonPath();
                    _previousElement = path.Current;

                    while (reader.Read())
                        switch (reader.TokenType)
                        {
                            case JsonToken.StartObject:
                                if (path.Current.IsPrimitiveType)
                                    path.Current.Type = JsonPathElement.ElementType.Complex;
                                else if (path.Current.IsObjectArray && path.Current.IsComplexTypeArray == false)
                                    path.Current.IsComplexTypeArray = true;
                                CreateObject(path);
                                break;
                            case JsonToken.StartArray:
                                path.Current.Type = JsonPathElement.ElementType.Array;
                                break;
                            case JsonToken.PropertyName:
                                var propertyName = (string) reader.Value;
                                if (context.PropertyNameList.Contains(propertyName))
                                {
                                    path.GoTo(propertyName);
                                }
                                else
                                { //To support complex types
                                    path.GoTo(propertyName);
                                    reader.Read();
                                    object value;
                                    switch (reader.TokenType)
                                    {
                                        case JsonToken.StartArray:
                                            value = JArray.Load(reader)?.ToString();
                                            break;
                                        case JsonToken.StartObject:
                                            value = JObject.Load(reader)?.ToString();
                                            break;
                                        default:
                                            value = reader.Value;
                                            break;
                                    }
                                    AddValue(path, value);
                                    path.GoBack();
                                }
                                break;
                            case JsonToken.EndObject:
                                if (path.Current.IsComplexType)
                                {
                                    CloseObject(path);
                                    if (path.Level > 0)
                                        path.GoBack();
                                }
                                else if (path.Current.IsComplexTypeArray)
                                {
                                    CloseObject(path);
                                }
                                else
                                {
                                    throw new ArgumentException(nameof(reader));
                                }
                                break;
                            case JsonToken.EndArray:
                                if (path.Current.IsObjectArray)
                                {
                                    CloseArray(path);
                                    if (path.Level > 0) path.GoBack();
                                }
                                else
                                {
                                    throw new ArgumentException(nameof(reader));
                                }
                                break;
                            case JsonToken.Integer:
                            case JsonToken.Float:
                            case JsonToken.String:
                            case JsonToken.Boolean:
                            case JsonToken.Null:
                            case JsonToken.Date:
                            case JsonToken.Bytes:
                                AddValue(path, reader.Value);
                                path.GoBack();
                                break;
                        }
                }
            }
            OnJsonRead();
        }

        #region Auxiliary Methods

        private void CloseArray(JsonPath path)
        {
            path.Current.MoveToParent();
        }

        private void CreateObject(JsonPath path)
        {
            var currentElement = path.Current;
            if (currentElement.IsRoot)
            {
                currentElement.Clear();
                var row = new JsonRow(_columns);
                row.AddValues(currentElement.FullName, new List<object>());
                currentElement.AddAffectedRow(row, true);
                _currentRow = row;
                _rows.Add(row);
            }
            else if (_previousElement == currentElement)
            {
                _copyRows = true;
            }
            else if (currentElement.Parent == _previousElement)
            {
                if (_copyRows)
                {
                    CopyRows(currentElement.Parent);
                    _copyRows = false;
                }

                var buffer = new List<object>();
                foreach (var affectedRow in currentElement.Parent.GetAffectedRows())
                    if (affectedRow.Value)
                    {
                        var row = affectedRow.Key;
                        row.AddValues(currentElement.FullName, buffer);
                        currentElement.AddAffectedRow(row, true);
                        _currentRow = row;
                    }
            }
            else
            {
                if (_copyRows)
                {
                    CopyRows(currentElement.Parent);
                    _copyRows = false;
                    var buffer = new List<object>();
                    var parent = currentElement.Parent;
                    foreach (var affectedRow in parent.GetAffectedRows())
                        if (affectedRow.Value)
                        {
                            var row = affectedRow.Key;
                            row.AddValues(currentElement.FullName, buffer);
                            if (!currentElement.AffectedRows.ContainsKey(row)) currentElement.AddAffectedRow(row, true);
                            _currentRow = row;
                        }
                }
                else
                {
                    foreach (var affectedRow in currentElement.GetAffectedRows())
                        currentElement.AffectedRows[affectedRow.Key] = false;
                    CopyRowsFromParent(currentElement);
                }
            }

            _previousElement = currentElement;
        }

        private static void CloseObject(JsonPath path)
        {
            if (!path.Current.IsComplexTypeArray) path.Current.MoveToParent();
        }

        private void AddValue(JsonPath path, object value)
        {
            if (_copyRows)
            {
                CopyRows(path.Current.Parent);
                _copyRows = false;
            }

            _currentRow[path] = value;
        }

        private void CopyRows(JsonPathElement currentElement)
        {
            var buffer = new List<object>();
            foreach (var affectedRow in currentElement.GetAffectedRows())
            {
                var row = affectedRow.Key;
                if (!affectedRow.Value) continue;
                currentElement.AffectedRows[affectedRow.Key] = false; //To set has used
                var newRow = new JsonRow(_columns);
                foreach (var name in currentElement.GetAffectedRows(false))
                {
                    if (name.StartsWith(currentElement.FullName)) continue;
                    if (row.TryGetValue(name, out var values)) newRow.AddValues(name, values);
                }

                newRow.AddValues(currentElement.FullName, buffer);
                currentElement.AddAffectedRow(newRow, true);
                _currentRow = newRow;
                _rows.Add(newRow);
            }
        }

        private void CopyRowsFromParent(JsonPathElement currentElement)
        {
            var buffer = new List<object>();
            foreach (var affectedRow in currentElement.Parent.GetAffectedRows())
            {
                var row = affectedRow.Key;
                if (affectedRow.Value)
                {
                    var newRow = row;
                    if (currentElement.AffectedRows.ContainsKey(row))
                    {
                        newRow = new JsonRow(_columns);
                        foreach (var name in currentElement.GetAffectedRows(false))
                        {
                            if (name.StartsWith(currentElement.FullName)) continue;
                            if (row.TryGetValue(name, out var values)) newRow.AddValues(name, values);
                        }

                        _rows.Add(newRow);
                    }

                    newRow.AddValues(currentElement.FullName, buffer);
                    currentElement.AddAffectedRow(newRow, true);
                    _currentRow = newRow;
                }
            }
        }

        #endregion
    }
}