using MG.CB.Metadata.MetaModel.Classes;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBGmailConnectorSample.Metadata
{
    /// <summary> Provides a base class for defining a SQL column w/ additional information. </summary>
    public class Column : Property, IColumn
    {
        public Column(string name, IDataType type, bool isNullable, int ordinal) : base(name, type, isNullable, ordinal)
        {
        }

        #region IColumn Members

        public bool IsUnique { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoincrementable { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSortable { get; set; }

        #endregion

        /// <summary> Gets or sets the full name of this instance [Path]. </summary>
        public string Path { get; set; }
    }
}