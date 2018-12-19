using System.Linq;
using CB.Connector.Exceptions;
using CBTestConnector.Command.Builder;
using CBTestConnector.Command.Helpers;
using CBTestConnector.Command.Translator;
using CBTestConnector.Command.Visitors;
using CBTestConnector.Connector;
using MG.CB.Command.DataHandler;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBTestConnector.Command.Handlers
{
    public class SelectSinkHandler : SelectSink, IHandler
    {
        public SelectSinkHandler(ExecutionSession session)
        {
            Session = session;
        }

        protected ExecutionSession Session { get; }
        
        protected override bool OnInit()
        {
            return true;
        }

        /// <summary> Defines the method to be called when the command is invoked. </summary>
        /// <remarks> Executes the query and builds an <see cref="IResultSetLoader"/>.</remarks>
        /// <param name="loader">The loader used to load a valid result expected by the CB Server.</param>
        /// <param name="context">The contextual information populated by the server before execution.</param>
        /// <remarks>
        /// CB Server invokes this method to retrieve data.
        /// <c>Note</c> This method has to always fill <see cref="IResultSetLoader"/> object.
        /// </remarks>
        public override void Execute(IResultSetLoader loader, IExecutionContext context)
        {
            ExecuteInternal(loader, context);
            //Prepares Sql Command
            var sqlCommandBuilder = new SqlCommandBuilder(Session.CommandInfo);
            var queryString = sqlCommandBuilder.GetSelectCommand();
            //Performs request to Data Source
            //Fetches data
            SqlDataLoader.LoadData(loader, context, ResultColumns, Session.ConnectionString, queryString);
        }

        public void ExecuteInternal(IResultSetLoader loader, IExecutionContext context)
        {
            //Calls for data from directly connected handler
            if (Previous != null && Previous is IHandler handler) handler.ExecuteInternal(loader, context);
            //Handles the RESULT COLUMNS in a parsed statement.
            var sourceColumns = ResultColumns.Where(p => p.IsSourceColumn).Select(arg => SqlTranslator.Instance.Translate(arg, null)).ToList();
            var metadata = DataHandlerVisitor.Instance.Visit(this);
            bool allSelected = false;
            switch (metadata)
            {
                case ITable table:
                    allSelected = sourceColumns.Count == table.Columns.Count;
                    break;
                case IFunction function:
                    allSelected = sourceColumns.Count == ((ITableType) function.Type).Elements.Count;
                    break;
            }
            Session.CommandInfo.Select = allSelected ? Constants.SymbolStar : string.Join(",", sourceColumns);
        }
    }
}