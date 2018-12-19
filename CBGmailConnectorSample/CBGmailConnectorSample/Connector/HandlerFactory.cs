using CBGmailConnectorSample.Command;
using MG.CB.Command.DataHandler;
using MG.CB.Connector.Classes;

namespace CBGmailConnectorSample.Connector
{
    //Creates CB SQL Data Manipulation Commands (e.g. Select, Where, Limit, Distinct, etc.) supported by your connection target.
    public class HandlerFactory : DataHandlerFactory
    {
        protected ExecutionSession Session { get; }

        public HandlerFactory(ExecutionSession session)
        {
            Session = session;
        }

        public override bool TryCreate(out CrossJoin handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out FullJoin handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out InnerJoin handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out LeftJoin handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out RightJoin handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out SubQuery handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out TableSource handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out TableFunctionSource handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out Where handler)
        {

            handler = null;
            return false;
        }
        
        public override bool TryCreate(out Having handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out OrderBy handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out Distinct handler)
        {

            handler = null;
            return false;
        }

        public override bool TryCreate(out Limit handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out Aggregate handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out SelectSink handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out InsertSink handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out UpdateSink handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out DeleteSink handler)
        {
            handler = null;
            return false;
        }

        public override bool TryCreate(out ExecuteSink handler)
        {
            handler = new ExecuteSinkHandler(Session);
            return true;
        }
    }
}