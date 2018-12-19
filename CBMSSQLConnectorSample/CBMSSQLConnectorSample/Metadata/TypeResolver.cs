using System;
using System.Collections.Generic;
using MG.CB.Metadata.MetaModel.Factories;
using MG.CB.Metadata.MetaModel.Helpers;
using MG.CB.Metadata.MetaModel.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace CBTestConnector.Metadata
{
    /// <summary> Provides methods to get <see cref="IDataType"/> from <see cref="DataType"/>. </summary>
    public static class TypeResolver
    {
        private const string Default = "text";

        public static readonly IDictionary<Type, SupportedType> FromSystemTypeToSupportedType =
            new Dictionary<Type, SupportedType>()
        {
            { typeof(long),         SupportedType.Int64    },
            { typeof(int),          SupportedType.Int32    },
            { typeof(short),        SupportedType.Int16    },
            { typeof(bool),         SupportedType.Boolean  },
            { typeof(char),         SupportedType.Char     },
            { typeof(string),       SupportedType.String   },
            { typeof(Guid),         SupportedType.String   }, //CB Server does not support GUID Type
            { typeof(DateTime),     SupportedType.DateTime },
            { typeof(TimeSpan),     SupportedType.String   }, //CB Server does not support TimeSpan Type
            { typeof(decimal),      SupportedType.Decimal  },
            { typeof(double),       SupportedType.Double   },
            { typeof(float),        SupportedType.Single   },
            { typeof(byte),         SupportedType.Byte     },
            { typeof(sbyte),        SupportedType.SByte    },
            { typeof(byte[]),       SupportedType.ByteArray}
        };


        public static readonly IDictionary<string, Type> FromStringToSystemType =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            {"bit",             typeof(bool)    },
            {"tinyint",         typeof(byte)    },
            {"bigint",          typeof(long)    },
            {"int",             typeof(int)     },
            {"smallint",        typeof(short)   },
            {"char",            typeof(char)    },
            {"varchar",         typeof(string)  },
            {"varcharmax",      typeof(string)  },
            {"xml",             typeof(string)  },
            {"nchar",           typeof(string)  },
            {"text",            typeof(string)  },
            {"ntext",           typeof(string)  },
            {"nvarchar",        typeof(string)  },
            {"nvarcharmax",     typeof(string)  },
            {"datetime",        typeof(DateTime)},
            {"smalldatetime",   typeof(DateTime)},
            {"date",            typeof(DateTime)},
            {"datetimeoffset",  typeof(DateTime)},
            {"datetime2",       typeof(DateTime)},
            {"decimal",         typeof(decimal) },
            {"money",           typeof(decimal) },
            {"smallmoney",      typeof(decimal) },
            {"real",            typeof(float)  },
            {"numeric",         typeof(decimal) },
            {"float",           typeof(double)  },
            {"timestamp",       typeof(byte[])  },
            {"binary",          typeof(byte[])  },
            {"varbinary",       typeof(byte[])  },
            {"varbinarymax",    typeof(byte[])  },
            {"rowversion",      typeof(byte[])  },
            {"image",           typeof(byte[])  },
            {"uniqueidentifier",typeof(Guid)    },
            {"time",            typeof(TimeSpan)}
        };

        /// <summary> Gets the definition of this primitive type reference. </summary>
        /// <param name="type">The primitive type reference.</param>
        /// <param name="database">The database.</param>
        /// <returns>Definition of the primitive type reference.</returns>
        public static IPrimitiveType GetType(DataType type, Database database)
        {
            string sqlType = Default;
            if (type.SqlDataType != SqlDataType.None)
            {
                sqlType = type.SqlDataType == SqlDataType.UserDefinedDataType 
                    ? database.UserDefinedDataTypes[type.Name].SystemType
                    : type.SqlDataType.ToString();
                sqlType = sqlType.ToLower();
            }

            var systemType = FromStringToSystemType.ContainsKey(sqlType) ? FromStringToSystemType[sqlType] : FromStringToSystemType[Default];
            var supportedType = FromSystemTypeToSupportedType[systemType];

            object[] args = null;
            if (systemType == typeof(string) || systemType == typeof(byte[]))
            {
                var maximumLength = type.MaximumLength;
                if (maximumLength == -1 || sqlType == "ntext" || sqlType == "text")
                {
                    maximumLength = int.MaxValue;
                }
                args = new object[] { maximumLength };
            }
            else if (systemType == typeof(decimal))
            {
                args = new object[] { type.NumericPrecision, type.NumericScale };
            }
            return args == null ? PrimitiveTypesFactory.Instance.Create(supportedType) : PrimitiveTypesFactory.Instance.Create(supportedType, args);
        }
    }
}