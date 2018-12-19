using CBTestConnector.Command.Handlers;
using MG.CB.Command.DataHandler;
using MG.CB.Command.DataHandler.Interfaces;
using MG.CB.Connector.Classes;

namespace CBTestConnector.Connector
{
    /// <summary> Factory implementation to manufacture new instances of classes that implement the <see cref="IDataHandler"/> interface. </summary>
    /// <seealso cref="MG.CB.Connector.Classes.DataHandlerFactory" />
    public class HandlerFactory : DataHandlerFactory
    {
        private readonly bool _provideOnlyBasicHandlers;

        public HandlerFactory(ExecutionSession session, bool provideOnlyBasicHandlers = true)
        {
            Session = session;
            _provideOnlyBasicHandlers = provideOnlyBasicHandlers;
        }

        protected ExecutionSession Session { get; }

        public override bool TryCreate(out CrossJoin handler)
        {//For instance, CBTestConnector does not support CROSS JOIN
            handler = null;
            return false;
        }

        public override bool TryCreate(out FullJoin handler)
        {
            handler = _provideOnlyBasicHandlers ? null : new FullJoinHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out InnerJoin handler)
        {//CBTestConnector supports INNER JOIN 
            handler = _provideOnlyBasicHandlers ? null: new InnerJoinHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out LeftJoin handler)
        {
            handler = _provideOnlyBasicHandlers ? null : new LeftJoinHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out RightJoin handler)
        {
            handler = _provideOnlyBasicHandlers ? null : new RightJoinHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out SubQuery handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out TableSource handler)
        {
            handler = new TableSourceHandler(Session);
            return true;
        }

        public override bool TryCreate(out TableFunctionSource handler)
        {
            handler = new TableFunctionSourceHandler(Session);
            return true;
        }

        public override bool TryCreate(out Where handler)
        {            
            handler = _provideOnlyBasicHandlers ? null : new WhereHandler(Session);
            return !_provideOnlyBasicHandlers;
        }
        
        public override bool TryCreate(out Having handler)
        {
            handler = _provideOnlyBasicHandlers ? null : new HavingHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out OrderBy handler)
        {
            handler = _provideOnlyBasicHandlers ? null : new OrderByHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out Distinct handler)
        {            
            handler = _provideOnlyBasicHandlers ? null : new DistinctHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out Limit handler)
        {
            handler = _provideOnlyBasicHandlers ? null : new LimitHandler(Session);
            return !_provideOnlyBasicHandlers;            
        }

        public override bool TryCreate(out Aggregate handler)
        {
            handler = _provideOnlyBasicHandlers ? null : new AggregateHandler(Session);
            return !_provideOnlyBasicHandlers;            
        }

        public override bool TryCreate(out SelectSink handler)
        {            
            handler = _provideOnlyBasicHandlers ? null : new SelectSinkHandler(Session);
            return !_provideOnlyBasicHandlers;
        }

        public override bool TryCreate(out UpdateSink handler)
        {
            handler = new UpdateSinkHandler(Session);
            return true;
        }

        public override bool TryCreate(out DeleteSink handler)
        {
            handler = new DeleteSinkHandler(Session);
            return true;
        }

        public override bool TryCreate(out InsertSink handler)
        {
            handler = new InsertSinkHandler(Session);
            return true;
        }

        public override bool TryCreate(out ExecuteSink handler)
        {
            handler = new ExecuteSinkHandler(Session);
            return true;
        }
    }
}