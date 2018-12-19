using System;
using System.Data.SqlClient;
using CB.Connector.Exceptions;
using MG.CB.Connector;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace CBTestConnector.Connector
{
    public class Connector : BaseConnector<Properties, ExecutionSession>
    {
        public override string Name { get; } = "CBTestConnector";
        public override string Description { get; } = "SQL Database Connector";
        public override string Author { get; } = "Connecting Software";
        
        public override Properties TestConnection(Properties properties)
        {
            try
            {
                using (var conn = new SqlConnection(properties.ConnectionString))
                {

                    var database = new Server(new ServerConnection(conn)).Databases[conn.Database];
                    database.AutoClose = true;
                    return ConnectorProperties;
                }
            }
            catch (Exception e)
            {
                throw ConnectorExceptionFactory.Create(ConnectorExceptionType.TestConnectionException, e);
            }
        }
        
        #region IDisposable

        private bool _disposed;
        
        /// <summary> Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary> Finalizes an instance of the <see cref="BaseConnector{TProperty,TSession}.Sessions" /> class. </summary>
        ~Connector()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
                foreach (var session in Sessions)
                    session?.Dispose();
            _disposed = true;
        }

        #endregion
    }
}