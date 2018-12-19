namespace CBGmailConnectorSample.Metadata
{
    /// <summary> Indicates whether this parameter goes in the query, path or request body for REST requests. </summary>
    public enum Location
    {
        RequestBody = 0,
        Query = 1,
        UrlSegment = 2,
        Unknown = 3
    }
}