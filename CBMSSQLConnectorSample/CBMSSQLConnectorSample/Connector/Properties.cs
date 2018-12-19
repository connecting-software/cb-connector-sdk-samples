using MG.CB.Connector.Properties;

namespace CBTestConnector.Connector
{
    public class Properties : ConnectorProperties<Properties>
    {
        [ConnectorProperty(
            Key = "ConnectionString",
            Name = "ConnectionString",
            Description =
                "A string that specifies information about a data source and the means of connecting to it.",
            Flags = ConnectorPropertyFlags.None,
            IsEncrypted = false )]
        public string ConnectionString { get; set; }

        protected override string GetKey()
        {
            return string.Empty;
        }
    }
}