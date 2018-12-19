using System;
using CBTestConnector.Command.Builder;
using CBTestConnector.Command.Helpers;
using CBTestConnector.Command.Translator;
using CBTestConnector.Connector;
using MG.CB.Command.DataHandler;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;
using Join = MG.CB.Command.DataHandler.Join;

namespace CBTestConnector.Command.Handlers
{
    public class LeftJoinHandler : LeftJoin, IHandler
    {
        public LeftJoinHandler(ExecutionSession session)
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
            //Handles the INNER JOIN in a parsed statement.
            Session.CommandInfo.Join = new Connector.Join
            {
                Left = Handle(Arguments.Left),
                Right = Handle(Arguments.Right),
                OnCriteria = SqlTranslator.Instance.Translate(Arguments.OnCriteria, null),
                JoinType = Arguments.Type.ToString()
            };
        }

        #region Auxiliary Methods

        /// <summary> Handles the current data source in a parsed statement. </summary>
        private static object Handle(DataSource dataSource)
        {
            switch (dataSource)
            {
                case TableSource source:
                    var tableSource = string.Compare(source.Arguments.Metadata.Name, source.Name, StringComparison.OrdinalIgnoreCase) != 0
                        ? $"{source.Arguments.Metadata.Owner.Name}.{source.Arguments.Metadata.Name} AS {source.Name}"
                        : $"{source.Arguments.Metadata.Owner.Name}.{source.Arguments.Metadata.Name}";
                    return tableSource;
                case Join source:
                    return new Connector.Join
                    {
                        Left = Handle(source.Arguments.Left),
                        Right = Handle(source.Arguments.Right),
                        OnCriteria = SqlTranslator.Instance.Translate(source.Arguments.OnCriteria, null),
                        JoinType = source.Arguments.Type.ToString()
                    };
                default: throw new ArgumentException(nameof(dataSource));
            }
        }

        #endregion

    }
}