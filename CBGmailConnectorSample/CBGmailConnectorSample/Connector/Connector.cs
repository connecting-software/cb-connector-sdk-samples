using System;
using CB.Connector.Exceptions;
using MG.CB.Connector;
using MG.CB.Connector.Properties.Extensions;
using RestSharp;

namespace CBGmailConnectorSample.Connector
{
    /*
     * CB Connector acts as a relational database that stores a collection of data and metadata. 
     * Responsibilities: 
     *  1. Enables to connect to a data source and to retrieve data
     *  2. Instantiate new instance of Session, which provides the relational data and SQL data manipulation commands
     *  3. Test the connection with the connection target
     *  4. Entry point for CB Server
     */
    public class Connector : BaseConnector<Properties, ExecutionSession>
    {
        #region IDisposable Members

        private bool _disposed;
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
                foreach (var session in Sessions)
                    session?.Dispose();
            _disposed = true;
        }

        #endregion

        // This method is used to test the connection with the connection target
        public override Properties TestConnection(Properties properties)
        {
            //Obtain a renewed access token 
            properties.RefreshOAuth2Properties();

            try
            {
                //Perform an operation against the connection target using the connection properties;
                //Ensures that the OAuth flow can execute properly;
                var client = new RestClient("https://www.googleapis.com/gmail/v1/")
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(ConnectorProperties.AccessToken, ConnectorProperties.TokenType)
                };
                var request = new RestRequest("/users/me/profile");
                var response = client.Execute(request);

                if ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 399)
                    return properties;

                throw ConnectorExceptionFactory.Create(ConnectorExceptionType.TestConnectionException, response.ErrorMessage);
            }
            catch (Exception e)
            {
                throw ConnectorExceptionFactory.Create(ConnectorExceptionType.TestConnectionException, e);
            }
        }

        // This property SHOULD be set to MSolDev1, MSolDev2 or MSolDev3 if your connector does not belong to the CB licensing system.
        public override string Name => "MSolDev3";

        public override string Description => "Read and send messages, manage drafts and attachments, search threads and messages, work with labels, setup push notifications, and manage Gmail settings.";

        public override string Author => "Connecting Software";

    }
}
