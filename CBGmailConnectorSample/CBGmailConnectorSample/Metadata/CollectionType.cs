using System;
using MG.CB.Metadata.MetaModel.Classes;
using MG.CB.Metadata.MetaModel.Interfaces;
using MG.Server.Core.MgsAPI;

namespace CBGmailConnectorSample.Metadata
{
    /// <summary> Wraps type array as CB primitive type. </summary>
    public class CollectionType : PrimitiveType
    {
        public CollectionType(IDataType type) : base("MgString", null, null, 8100, 2 * 8100, (short)OdbcSqlTypes.SQL_VARCHAR, 8100, 1, null)
        {
            Type = type;
        }

        public IDataType Type { get; }
        public override Type ClrType => typeof(string);
    }
}