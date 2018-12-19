using System;
using RestSharp;

namespace CBGmailConnectorSample.Metadata
{
    public static class ExtensionMethods
    {
        public static Method Parse(this string _this)
        {
            switch (_this)
            {
                case "POST": return Method.POST;
                case "GET": return Method.GET;
                case "DELETE": return Method.DELETE;
                case "PATCH": return Method.PATCH;
                case "PUT": return Method.PUT;
                default: throw new ArgumentException(nameof(_this));
            }
        }
    }
}