using System;
using System.Collections.Generic;

namespace CBGmailConnectorSample.Result.Json
{
    /// <summary>
    ///     Provides a base class for a segment within a <see cref="JsonPath" />.
    /// </summary>
    public class JsonPathElement
    {
        /// <summary>
        ///     Specifies the type of JSON path element.
        /// </summary>
        public enum ElementType
        {
            /// <summary>
            ///     Represents a primitive element.
            /// </summary>
            Primitive = 0,

            /// <summary>
            ///     Represents a complex element.
            /// </summary>
            Complex = 1,

            /// <summary>
            ///     Represents an array element.
            /// </summary>
            Array = 2
        }

        #region Fields

        private bool _isComplexTypeArray;

        #endregion

        internal IDictionary<JsonRow, bool> AffectedRows = new Dictionary<JsonRow, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPathElement"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="builder">The builder.</param>
        public JsonPathElement(string name, JsonPathBuilder builder) : this(null, name, ElementType.Primitive, builder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPathElement"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        public JsonPathElement(JsonPathElement parent, string name) : this(parent, name, ElementType.Primitive, null)
        {
        }

        internal JsonPathElement(JsonPathElement parent, string name, ElementType type, JsonPathBuilder builder)
        {
            Parent = parent;
            if (Parent == null)
            {
                Builder = builder ?? throw new ArgumentNullException(nameof(builder));
                Level = 0;
                Name = Builder.Insert(name ?? throw new ArgumentNullException(nameof(name)));
                FullName = Name;
            }
            else
            {
                Builder = Parent.Builder;
                Level = Parent.Level + 1;
                Name = Builder.Insert(name ?? throw new ArgumentNullException(nameof(name)));
                FullName = Builder.Insert($"{Parent.FullName}{Builder.PathDelimiter}{Name}");
            }

            Type = type;
        }

        /// <summary> Gets the name. </summary>
        public string Name { get; }

        /// <summary> Gets the full name. </summary>
        public string FullName { get; }

        /// <summary> Gets the level. </summary>
        public int Level { get; }

        /// <summary> Gets a value indicating whether this instance is root. </summary>
        /// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
        public bool IsRoot => Parent == null;

        /// <summary> Gets a value indicating whether this instance represents an object array. </summary>
        /// <value> <c>true</c> if this instance is object array; otherwise, <c>false</c>. </value>
        public bool IsObjectArray => Type == ElementType.Array;

        /// <summary> Gets a value indicating whether this instance represents a complex type. </summary>
        /// <value>   <c>true</c> if this instance is complex type; otherwise, <c>false</c>. </value>
        public bool IsComplexType => Type == ElementType.Complex;
        
        /// <summary> Gets a value indicating whether this instance represents a primitive type. </summary>
        /// <value>   <c>true</c> if this instance is complex type; otherwise, <c>false</c>. </value>
        public bool IsPrimitiveType => Type == ElementType.Primitive;

        /// <summary> Gets a value indicating whether this instance represents a complex type array. </summary>
        /// <value>   <c>true</c> if this instance is complex type; otherwise, <c>false</c>. </value>
        public bool IsComplexTypeArray
        {
            get => IsObjectArray && _isComplexTypeArray;
            set
            {
                if (IsObjectArray) _isComplexTypeArray = value;
            }
        }

        /// <summary> Gets the element type of this instance. </summary>
        public ElementType Type { get; set; }

        /// <summary> Gets the JSON path builder. </summary>
        public JsonPathBuilder Builder { get; }

        /// <summary> Gets the parent. </summary>
        public JsonPathElement Parent { get; }

        /// <summary> Gets the child. </summary>
        public JsonPathElement Child { get; private set; }

        /// <summary> Gets a list of affected objects. </summary>
        public List<string> AffectedObjects { get; internal set; }
        
        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="AffectedRows"/>.
        /// </summary>
        /// <param name="row">The object to use as the key of the element to add.</param>
        /// <param name="created">The object to use as the value of the element to add.</param>
        public void AddAffectedRow(JsonRow row, bool created)
        {
            AffectedRows.Add(row, created);
        }
        
        /// <summary> Gets a list of affected rows. </summary>
        /// <returns>A list of affected objects.</returns>
        public IDictionary<JsonRow, bool> GetAffectedRows()
        {
            IDictionary<JsonRow, bool> result = new Dictionary<JsonRow, bool>(AffectedRows);
            if (Child != null)
                foreach (var item in Child.GetAffectedRows())
                    result[item.Key] = item.Value;
            return result;
        }

        /// <summary> Gets a list of affected rows. </summary>
        /// <param name="createdOnly"><c>true</c> if the list of affected rows should contain the parent affected rows; otherwise, <c>false</c>.</param>
        /// <returns></returns>
        public IEnumerable<string> GetAffectedRows(bool createdOnly)
        {
            List<string> result;
            if (IsRoot)
            {
                result = new List<string> {FullName};
                if (createdOnly == false && AffectedObjects != null) result.AddRange(AffectedObjects);
            }
            else
            {
                result = (List<string>) Parent.GetAffectedRows(createdOnly);
                result.Add(FullName);
                if (createdOnly == false && AffectedObjects != null) result.AddRange(AffectedObjects);
            }

            return result;
        }

        /// <summary> Clears this instance. </summary>
        internal void Clear()
        {
            AffectedObjects = null;
            AffectedRows.Clear();
            Child = null;
        }

        /// <summary> Moves to parent. </summary>
        internal void MoveToParent()
        {
            if (IsRoot) return;
            if (Parent.AffectedObjects == null)
            {
                Parent.AffectedObjects = new List<string> {FullName};
            }
            else
            {
                if (!Parent.AffectedObjects.Contains(FullName)) Parent.AffectedObjects.Add(FullName);
            }

            if (AffectedObjects != null) Parent.AffectedObjects.AddRange(AffectedObjects);
            foreach (var affectedRow in AffectedRows)
                if (!Parent.AffectedRows.ContainsKey(affectedRow.Key))
                    Parent.AffectedRows.Add(affectedRow.Key, true);
            Clear();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}