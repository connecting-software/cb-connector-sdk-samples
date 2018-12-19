using System;

namespace CBGmailConnectorSample.Result.Json
{
    /// <summary>
    /// Provides a base class for defining a path to an element (or a set of elements) in a JSON structure.
    /// </summary>
    public class JsonPath
    {
        /// <summary> Initializes a new instance of the <see cref="JsonPath"/> class. </summary>
        public JsonPath()
        {
            Current = new JsonPathElement("", new JsonPathBuilder());
        }

        /// <summary> Gets the current <see cref="JsonPathElement"/>. </summary>
        public JsonPathElement Current { get; internal set; }

        /// <summary> Gets the level. </summary>
        public int Level => Current.Level;

        /// <summary> Go to the JSON property. </summary>
        /// <param name="element">The property name.</param>
        public void GoTo(string element)
        {
            Current = new JsonPathElement(Current, element);
        }

        /// <summary> Go back to the parent JSON property. </summary>
        public void GoBack()
        {
            if (Current.IsRoot) throw new IndexOutOfRangeException("Root node has no parent.");
            Current = Current.Parent;
        }

        /// <summary> Gets the path name of the parent. </summary>
        /// <returns>The path name of the parent.</returns>
        public string ParentPathName()
        {
            return Current.Parent.FullName;
        }

        /// <summary> Gets the path name of the current <see cref="JsonPathElement"/>. </summary>
        /// <returns>The path name of the current <see cref="JsonPathElement"/>.</returns>
        public string PathName()
        {
            return Current.FullName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Current.FullName;
        }
    }
}