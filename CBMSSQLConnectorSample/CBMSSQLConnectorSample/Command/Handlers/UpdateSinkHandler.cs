using System.Data.SqlClient;
using System.Linq;
using CBTestConnector.Command.Translator;
using CBTestConnector.Connector;
using MG.CB.Command.DataHandler;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;
using SqlCommandBuilder = CBTestConnector.Command.Builder.SqlCommandBuilder;

namespace CBTestConnector.Command.Handlers
{
    public class UpdateSinkHandler : UpdateSink, IHandler
    {
        public UpdateSinkHandler(ExecutionSession session)
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
            //Prepares Sql Command
            var sqlCommandBuilder = new SqlCommandBuilder(Session.CommandInfo);
            var queryString = sqlCommandBuilder.GetUpdateCommand();
            //Performs request to Data Source
            using (var conn = new SqlConnection(Session.ConnectionString))
            {
                conn.Open();
                using (SqlCommand sqlCommand = new SqlCommand(queryString, conn))
                {
                    var variables = context.GetAllVariables().ToList();
                    variables.ForEach(v =>
                    {
                        sqlCommand.Parameters.Add(new SqlParameter(v.Name, context.GetVariableValue(v)));
                    });
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        //Returns an empty result with the the number of rows changed by execution of the Transact-SQL statement.
                        loader.ReturnEmptyResult(null, reader.RecordsAffected, null);
                    }
                }
            }
        }

        public void ExecuteInternal(IResultSetLoader loader, IExecutionContext context)
        {
            //Calls for data from directly connected handler
            if (Previous != null && Previous is IHandler handler) handler.ExecuteInternal(loader, context);
            //Handles the SET in a parsed statement.
            foreach (var assignedPair in Arguments.Arguments.AssignedPairs)
            {
                var convertedColumnMetadata = SqlTranslator.Instance.Translate(assignedPair.Key, context);
                var convertedColumnValue = SqlTranslator.Instance.Translate(assignedPair.Key, context);

                Session.CommandInfo.Set.Add($"{convertedColumnMetadata} = {convertedColumnValue}");
            }
        }
    }
}