using System;
using System.Linq;
using CBTestConnector.Metadata;
using MG.CB.Connector;
using MG.CB.Connector.Classes;
using MG.CB.Metadata.MetaModel.Factories;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBTestConnector.Connector
{
    /// <summary> Provides an abstract class for defining a CB command session. </summary>
    /// <seealso cref="Session" />
    public class ExecutionSession : Session
    {
        public ExecutionSession(Connector connector) : base(connector)
        {
            string connString = connector.ConnectorProperties.ConnectionString;
            if (connString == null)
            {
                throw new Exception("ConnectionString property is missing!");
            }

            string targetDatabase = connString.Split(';').FirstOrDefault(x => x.StartsWith("Database", StringComparison.OrdinalIgnoreCase));
            if (targetDatabase == null)
            {
                throw new Exception("Database in ConnectionString property is missing!");
            }

            ConnectionString = connString;
            int assignmentPos = targetDatabase.IndexOf('=');
            if (assignmentPos > 0)
            {
                targetDatabase = targetDatabase.Substring(assignmentPos + 1);
            }
            
            //Loads MetaModel object.
            if (connector.CachingProvider.ContainsKey(targetDatabase))
            {
                MetaModel = (IMetaModel)connector.CachingProvider.GetItem(targetDatabase, DateTime.UtcNow.AddMinutes(10));
            }
            else
            {
                var loader = new SqlMetaDataLoader(this);
                MetaModel = LazyMetaModelFactory.Instance.CreateMetaModel(targetDatabase, "dbo", loader); 
                connector.CachingProvider.AddItem(MetaModel.Name, MetaModel, DateTime.UtcNow.AddMinutes(10));
            }

            HandlerFactory = new HandlerFactory(this, false);
            CommandInfo = new ContextSqlCommandInfo();
        }

        public string ConnectionString { get; }

        /// <summary> Contextual information for querying or editing data. </summary>
        public ContextSqlCommandInfo CommandInfo { get; }

        /// <summary> Metadata information. </summary>
        public sealed override IMetaModel MetaModel { get; }

        /// <summary> Factory that always creates SUPPORTED data handlers. </summary>
        public override IDataHandlerFactory HandlerFactory { get; }
    }
}