using System.Linq;
using CBTestConnector.Command.Builder;
using CBTestConnector.Command.Helpers;
using CBTestConnector.Command.Translator;
using CBTestConnector.Connector;
using MG.CB.Command.DataHandler;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;

namespace CBTestConnector.Command.Handlers
{
    public class OrderByHandler : OrderBy, IHandler
    {
        public OrderByHandler(ExecutionSession session)
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
            //Handles the RESULT COLUMNS in a parsed statement.
            Session.CommandInfo.Select = Constants.SymbolStar;
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
            //Handles the ORDER BY clause in a parsed statement.
            var itemsToOrder = Arguments.Items.Select(arg => SqlTranslator.Instance.Translate(arg.Argument, context) + " " + arg.Sort);
            Session.CommandInfo.OrderBy = string.Join(",", itemsToOrder);
        }
    }
}