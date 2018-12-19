using MG.CB.Metadata.MetaModel.Classes;
using MG.CB.Metadata.MetaModel.Interfaces;
using MG.Server.Core.MgsAPI;

namespace CBGmailConnectorSample.Metadata
{
    /// <summary> Wraps type ref__ as CB primitive type. </summary>
    public class ComplexType : PrimitiveType
    {
        public ComplexType(IComplexType structuredType) : base("MgString", null, null, 8100, 2 * 8100, (short)OdbcSqlTypes.SQL_VARCHAR, 8100, 1, null)
        {
            StructuredType = structuredType;
        }

        public override System.Type ClrType => typeof(string);

        public IComplexType StructuredType { get; }
    }
}