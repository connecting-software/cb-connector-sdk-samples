using System;
using System.Collections.Generic;
using MG.CB.Metadata.MetaModel.Classes;
using MG.Server.Core.MgsAPI;

namespace CBGmailConnectorSample.Metadata
{
    /// <summary> Provides a base class for defining an enum type. Wraps enum type as CB primitive type. </summary>
    public class EnumType : PrimitiveType
    {
        public EnumType() : base("MgString", null, null, 8100, 2 * 8100, (short) OdbcSqlTypes.SQL_VARCHAR, 8100, 1, null)
        {
        }


        public ICollection<string> Members { get; } = new List<string>();

        public override Type ClrType => typeof(string);

        public override object Convert(object obj)
        {
            if (!Members.Contains(obj.ToString()))
                throw new InvalidOperationException($"{obj} does not match any key in the {GetType().Name} members.");
            return base.Convert(obj);
        }
    }
}