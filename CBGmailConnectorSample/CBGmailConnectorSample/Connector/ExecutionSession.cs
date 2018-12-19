using System;
using CBGmailConnectorSample.Metadata;
using MG.CB.Connector;
using MG.CB.Connector.Classes;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBGmailConnectorSample.Connector
{
    /*
     * Session acts as a CB SQL command session that is occurrence of an user interacting with your connection target. 
     * Responsibilities: 
     *  1. Instantiate new instance of MetaModel - provides the relational data
     *  2. Instantiate new instance of DataHandlerFactory - provides SQL data manipulation commands.
     */
    public class ExecutionSession : Session
    {
        public ExecutionSession(Connector connector) : base(connector)
        {
            // Models your connection target as a database.
            // Use of in-memory cache that allow improving the performance by reducing the effort required to generate content.
            const string cacheKey = "googleapis.gmail.v1";
            if (connector.CachingProvider.ContainsKey(cacheKey))
            {
                MetaModel = (IMetaModel)connector.CachingProvider.GetItem(cacheKey, DateTime.Now.AddSeconds(60));
            }
            else
            {
                MetaModel = new Builder().GetModel();
                connector.CachingProvider.AddItem(cacheKey, MetaModel, DateTime.Now.AddSeconds(60));
            }
            // Create factory responsible for instantiating the CB SQL Data Manipulation Commands supported by your connection target
            HandlerFactory = new HandlerFactory(this);
        }

        public sealed override IMetaModel MetaModel { get; }
        public override IDataHandlerFactory HandlerFactory { get; }

    }
}
