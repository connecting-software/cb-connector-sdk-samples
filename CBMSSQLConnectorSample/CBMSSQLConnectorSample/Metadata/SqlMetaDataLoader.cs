using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CBTestConnector.Connector;
using MG.CB.Metadata.MetaModel.Factories;
using MG.CB.Metadata.MetaModel.Interfaces;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace CBTestConnector.Metadata
{
    /// <summary> Retrieves metadata information from <see cref="Database"/> object. </summary>
    public class SqlMetaDataLoader : ILoader
    {
        private readonly LazyMetaModelFactory _modelFactory = LazyMetaModelFactory.Instance;
        private readonly PrimitiveTypesFactory _typesFactory = PrimitiveTypesFactory.Instance;

        private static readonly HashSet<string> AggregateFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "count", "avg", "min", "max", "checksum_agg", "sum", "stdev", "count_big", "stdevp", "grouping",
            "string_agg", "grouping_id", "var", "varp"
        };

        private static readonly HashSet<string> BuiltInScalarFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"add", "sub", "div", "mult", "mod"};

        public SqlMetaDataLoader(ExecutionSession session)
        {
            using (var conn = new SqlConnection(session.ConnectionString))
            {
                Database = new Server(new ServerConnection(conn)).Databases[conn.Database];
            }
        }

        public Database Database { get; }

        public bool TryLoadElement<T>(string name, out T element, object ghost)
        {
            try
            {
                return TryLoadElement(name, out element, (dynamic)ghost);
            }
            catch
            {
                element = default(T);
                return false;
            }
        }

        public bool TryLoadElements<T>(out ICollection<T> elements, object ghost)
        {
            try
            {
                return TryLoadElements(out elements, (dynamic)ghost);
            }
            catch
            {
                elements = null;
                return false;
            }
        }

        #region IMetaModel objects

        protected bool TryLoadElement<T>(string name, out T element, IMetaModel owner)
        {
            element = (T)_modelFactory.CreateSchema(name, this);
            return true;
        }

        protected bool TryLoadElements<T>(out ICollection<T> elements, IMetaModel owner)
        {
            elements = new List<T>();
            foreach (Schema sqlSchema in Database.Schemas)
            {
                elements.Add((T)_modelFactory.CreateSchema(sqlSchema.Name, this));
            }
            return true;
        }

        #endregion

        #region ISchema objects

        protected bool TryLoadElement<T>(string name, out T element, ISchema owner)
        {
            if (typeof(IProcedure).IsAssignableFrom(typeof(T)))
            {
                if (Database.StoredProcedures.Contains(name, owner.Name))
                {
                    element = (T)_modelFactory.CreateProcedure(name, this);
                    return true;
                }
                element = default(T);
                return false;
            }
            if (typeof(ITable).IsAssignableFrom(typeof(T)))
            {
                if (Database.Tables.Contains(name, owner.Name))
                {
                    element = (T)_modelFactory.CreateTable(name, this);
                    return true;
                }
                element = default(T);
                return false;
            }
            if (typeof(IView).IsAssignableFrom(typeof(T)))
            {
                if (Database.Views.Contains(name, owner.Name))
                {
                    element = (T)_modelFactory.CreateView(name, this);
                    return true;
                }
                element = default(T);
                return false;
            }
            if (typeof(IFunction).IsAssignableFrom(typeof(T)))
            {
                if (Database.UserDefinedFunctions.Contains(name, owner.Name))
                {
                    var function = Database.UserDefinedFunctions[name];
                    element = (T)_modelFactory.CreateFunction(name, (FunctionType)(int)function.FunctionType, this);
                    return true;
                }
                if (AggregateFunctions.Contains(name))
                {
                    element = (T)_modelFactory.CreateFunction(name, FunctionType.Aggregate, this);
                    return true;
                }
                if (BuiltInScalarFunctions.Contains(name))
                {
                    element = (T)_modelFactory.CreateFunction(name, FunctionType.ScalarValued, this);
                    return true;
                }
                element = default(T);
                return false;
            }

            element = default(T);
            return false;
        }

        protected bool TryLoadElements<T>(out ICollection<T> elements, ISchema owner)
        {
            elements = new List<T>();
            foreach (Table table in Database.Tables)
            {
                if (table.Schema != owner.Name) continue;
                elements.Add((T)_modelFactory.CreateTable(table.Name, this));
            }
            foreach (StoredProcedure procedure in Database.StoredProcedures)
            {
                if (procedure.Schema != owner.Name) continue;
                elements.Add((T)_modelFactory.CreateProcedure(procedure.Name, this));
            }
            foreach (View view in Database.Views)
            {
                if (view.Schema != owner.Name) continue;
                elements.Add((T)_modelFactory.CreateView(view.Name, this));
            }
            foreach (UserDefinedFunction function in Database.UserDefinedFunctions)
            {
                if (function.Schema != owner.Name) continue;
                elements.Add((T) _modelFactory.CreateFunction(function.Name, (FunctionType) (int) function.FunctionType,this));
            }
            foreach (string function in AggregateFunctions)
            {
                elements.Add((T)_modelFactory.CreateFunction(function, FunctionType.Aggregate, this));
            }
            foreach (string function in BuiltInScalarFunctions)
            {
                elements.Add((T)_modelFactory.CreateFunction(function, FunctionType.ScalarValued, this));
            }
            return true;
        }

        #endregion

        #region ITable objects

        protected bool TryLoadElement<T>(string name, out T element, ITable owner)
        {
            if (Database.Tables[owner.Name, owner.Owner.Name].Columns.Contains(name))
            {
                element = (T) GetColumn(Database.Tables[owner.Name, owner.Owner.Name].Columns[name]);
                return true;
            }

            element = default(T);
            return false;
        }

        protected bool TryLoadElements<T>(out ICollection<T> elements, ITable owner)
        {
            elements = new List<T>();
            foreach (Column column in Database.Tables[owner.Name, owner.Owner.Name].Columns)
            {
                elements.Add((T)GetColumn(column));
            }
            return true;
        }

        #endregion

        #region IProcedure objects

        protected bool TryLoadElement<T>(string name, out T element, IProcedure owner)
        {
            if (Database.StoredProcedures[owner.Name, owner.Owner.Name].Parameters.Contains(name))
            {
                element = (T)GetParameter(Database.StoredProcedures[owner.Name, owner.Owner.Name].Parameters[name]);
                return true;
            }

            element = default(T);
            return false;
        }

        protected bool TryLoadElements<T>(out ICollection<T> elements, IProcedure owner)
        {
            elements = new List<T>();
            foreach (Parameter parameter in Database.StoredProcedures[owner.Name, owner.Owner.Name].Parameters)
            {
                elements.Add((T)GetParameter(parameter));
            }
            return true;
        }

        #endregion

        #region IView objects

        protected bool TryLoadElement<T>(string name, out T element, IView owner)
        {
            if (Database.Views[owner.Name, owner.Owner.Name].Columns.Contains(name))
            {
                element = (T)GetColumn(Database.Views[owner.Name, owner.Owner.Name].Columns[name]);
                return true;
            }

            element = default(T);
            return false;
        }

        protected bool TryLoadElements<T>(out ICollection<T> elements, IView owner)
        {
            elements = new List<T>();
            foreach (Column column in Database.Views[owner.Name, owner.Owner.Name].Columns)
            {
                elements.Add((T)GetColumn(column));
            }
            return true;
        }

        #endregion

        #region IFunction objects

        protected bool TryLoadElement<T>(string name, out T element, IFunction owner)
        {
            if (Database.UserDefinedFunctions.Contains(owner.Name, owner.Owner.Name))
            {
                if (Database.StoredProcedures[owner.Name, owner.Owner.Name].Parameters.Contains(name))
                {
                    element = (T)GetParameter(Database.StoredProcedures[owner.Name, owner.Owner.Name].Parameters[name]);
                    return true;
                }
            }
            else if (AggregateFunctions.Contains(owner.Name))
            {
                switch (name)
                {
                    case "Expression":
                        element = (T)_modelFactory.CreateParameter(name,
                            _typesFactory.CreateAnyType(), false, 1, ParameterDirectionKind.In, true);
                        return true;
                    case "Return":
                        element = (T)_modelFactory.CreateParameter(name,
                            _modelFactory.CreateParametricType(p => p.First()), false, 2, ParameterDirectionKind.Return);
                        return true;
                    default:
                        element = default(T);
                        return false;
                }
            }
            else if (BuiltInScalarFunctions.Contains(owner.Name))
            {
                switch (name)
                {
                    case "Param1":
                        element = (T)_modelFactory.CreateParameter(name,
                            _typesFactory.CreateAnyType(), false, 1, ParameterDirectionKind.In, true);
                        return true;
                    case "Param2":
                        element = (T)_modelFactory.CreateParameter(name,
                            _typesFactory.CreateAnyType(), false, 2, ParameterDirectionKind.In, true);
                        return true;
                    case "Return":
                        element = (T)_modelFactory.CreateParameter(name,
                            _modelFactory.CreateParametricType(p => p.First()), false, 3, ParameterDirectionKind.Return);
                        return true;
                    default:
                        element = default(T);
                        return false;
                }
            }

            element = default(T);
            return false;
        }

        protected bool TryLoadElements<T>(out ICollection<T> elements, IFunction owner)
        {
            if (Database.UserDefinedFunctions.Contains(owner.Name, owner.Owner.Name))
            {
                elements = new List<T>();
                var function = Database.UserDefinedFunctions[owner.Name, owner.Owner.Name];
                foreach (Parameter parameter in function.Parameters)
                {
                    elements.Add((T)GetParameter(parameter));
                }
                // Resolves the Return Parameter
                switch (function.FunctionType)
                {
                    case UserDefinedFunctionType.Table:
                    {
                        var tableType = _modelFactory.CreateTableType(function.TableVariableName);
                        foreach (Column column in function.Columns) tableType.Elements.Add(GetColumn(column));
                        var @return = (T) _modelFactory.CreateParameter("Return", tableType, false,
                            elements.Count + 1, ParameterDirectionKind.Return);
                        elements.Add(@return);
                        break;
                    }
                    case UserDefinedFunctionType.Scalar:
                    {
                        var type = TypeResolver.GetType(function.DataType, Database);
                        var @return = (T) _modelFactory.CreateParameter("Return", type, false,
                            elements.Count + 1, ParameterDirectionKind.Return);
                        elements.Add(@return);
                        break;
                    }
                }
                return true;
            }
            if (AggregateFunctions.Contains(owner.Name))
            {
                elements = new List<T>
                {
                    (T) _modelFactory.CreateParameter("Expression", _typesFactory.CreateAnyType(),
                        false, 1, ParameterDirectionKind.In, true),
                    (T) _modelFactory.CreateParameter("Return", _typesFactory.CreateAnyType(), false,
                        2, ParameterDirectionKind.Return)
                };
                return true;
            }
            if (BuiltInScalarFunctions.Contains(owner.Name))
            {
                elements = new List<T>
                {
                    (T) _modelFactory.CreateParameter("Param1", _typesFactory.CreateAnyType(), false,
                        1, ParameterDirectionKind.In, true),
                    (T) _modelFactory.CreateParameter("Param2", _typesFactory.CreateAnyType(), false,
                        2, ParameterDirectionKind.In, true),
                    (T) _modelFactory.CreateParameter("Return", _typesFactory.CreateAnyType(), false,
                        3, ParameterDirectionKind.Return)
                };
                return true;
            }

            elements = null;
            return false;
        }

        #endregion

        #region Auxiliary Methods

        private IColumn GetColumn(Column column)
        {
            var type = TypeResolver.GetType(column.DataType, Database);
            var isNullable = column.Nullable;
            var ordinal = column.ID;
            var isPrimary = column.InPrimaryKey;
            var isForeignKey = column.IsForeignKey;
            var isUnique = isPrimary || isForeignKey;
            var isAutoincrementable = column.IdentityIncrement > 0;

             return _modelFactory.CreateColumn(column.Name,
                type,
                isNullable,
                ordinal,
                isUnique,
                isForeignKey,
                isPrimary,
                isAutoincrementable);
        }

        private IParameter GetParameter(Parameter parameter)
        {
            var type = TypeResolver.GetType(parameter.DataType, Database);
            var isReadOnly = parameter.IsReadOnly;
            var ordinal = parameter.ID;
            return _modelFactory.CreateParameter(parameter.Name, 
                type, 
                false, 
                ordinal, 
                ParameterDirectionKind.In, 
                isReadOnly);
        }

        #endregion
    }
}