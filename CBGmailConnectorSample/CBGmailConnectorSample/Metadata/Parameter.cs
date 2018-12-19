using MG.CB.Metadata.MetaModel.Classes;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBGmailConnectorSample.Metadata
{
    /// <summary> Provides a base class for defining a SQL parameter w/ additional information. </summary>
    public class Parameter : Property, IParameter
    {
        public Parameter(string name, IDataType type, bool isNullable, int ordinal) : base(name, type, isNullable, ordinal)
        {
            Direction = ParameterDirectionKind.In;
        }

        #region IParameter Members
        
        public ParameterDirectionKind Direction { get; internal set; }
        public bool IsReadOnly { get; set; }

        #endregion

        /// <summary> Indicates whether this parameter goes in the query, path or request body for REST requests. </summary>
        public Location Location { get; internal set; }
    }
}