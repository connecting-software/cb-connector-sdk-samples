using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CBTestConnector.Command.Translator;
using CBTestConnector.Connector;
using CBTestConnector.Metadata;
using MG.CB.Command.DataHandler;
using MG.CB.Command.DataHandler.Argument;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;
using MG.CB.Metadata.MetaModel.Factories;

namespace CBTestConnector.Command.Handlers
{
    public class ExecuteSinkHandler : ExecuteSink, IHandler
    {
        public ExecuteSinkHandler(ExecutionSession session)
        {
            Session = session;
        }

        protected ExecutionSession Session { get; }
        
        protected override bool OnInit()
        {
            return true;
        }

        public override void Execute(IResultSetLoader loader, IExecutionContext context)
        {
            ExecuteInternal(loader, context);
            //Handles the Parameters in a parsed statement.
            var parameters = SqlTranslator.Instance.Translate(Arguments.Parameters, context);
            //Prepares Sql Command
            var queryString = $"EXEC {Arguments.Procedure} {parameters}";
            //Performs request to Data Source
            using (var conn = new SqlConnection(Session.ConnectionString))
            {
                conn.Open();

                using (var sqlCommand = new SqlCommand(queryString, conn))
                {
                    var variables = context.GetAllVariables().ToList();
                    variables.ForEach(v =>
                    {
                        sqlCommand.Parameters.Add(new SqlParameter(v.Name, context.GetVariableValue(v)));
                    });
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        do
                        {
                            ResultColumns.Clear();
                            var table = reader.GetSchemaTable();
                            if (table == null) continue; //Means that procedure has no return type.
                            //When the procedure has return type, ResultColumns has to be set.
                            foreach (DataRow row in table.Rows)
                            {
                                var name = row["ColumnName"].ToString();
                                var ordinal = int.Parse(row["ColumnOrdinal"].ToString());
                                var isNullable = bool.Parse(row["AllowDBNull"].ToString());
                                var isUnique = bool.Parse(row["IsUnique"].ToString());
                                var isKey = false;
                                if (!string.IsNullOrEmpty(row["IsKey"].ToString())) isKey = bool.Parse(row["IsKey"].ToString());
                                var isAutoIncrement = bool.Parse(row["IsAutoIncrement"].ToString());
                                var isPrimaryKey = isKey && isAutoIncrement;
                                var isForeignKey = isKey && !isPrimaryKey;
                                var systemType = TypeResolver.FromStringToSystemType[row["DataTypeName"].ToString()];
                                var supportedType = TypeResolver.FromSystemTypeToSupportedType[systemType];
                                var type = PrimitiveTypesFactory.Instance.Create(supportedType);
                                var columnA = EagerMetaModelFactory.Instance.CreateColumn(name, type, isNullable,
                                    ordinal, isUnique, isForeignKey, isPrimaryKey, isAutoIncrement);
                                var columnSource = new ColumnSource(columnA);
                                ResultColumns.Add(columnSource);
                            }
                            if (ResultColumns.Count > 0)
                            {
                                using (var tableLoader = loader.OpenTableResultLoader(ResultColumns))
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read() && !tableLoader.CancellationToken.IsCancellationRequested)
                                        {
                                            var row = tableLoader.NewRow();
                                            reader.GetValues(row.GetValues());
                                            tableLoader.Add(row);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                loader.ReturnEmptyResult(null, reader.RecordsAffected, null);
                            }

                        } while (reader.NextResult());
                    }
                }
            }
        }

        public void ExecuteInternal(IResultSetLoader loader, IExecutionContext context)
        {
            //Calls for data from directly connected handler
            if (Previous != null && Previous is IHandler handler) handler.ExecuteInternal(loader, context);
        }
    }
}