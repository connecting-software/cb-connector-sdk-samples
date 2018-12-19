using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CB.Connector.Exceptions;
using CBGmailConnectorSample.Connector;
using CBGmailConnectorSample.Error;
using CBGmailConnectorSample.Metadata;
using CBGmailConnectorSample.Result;
using CBGmailConnectorSample.Result.Json;
using CBGmailConnectorSample.Translator;
using MG.CB.Command.DataHandler;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using Parameter = CBGmailConnectorSample.Metadata.Parameter;

namespace CBGmailConnectorSample.Command
{
    public class ExecuteSinkHandler : ExecuteSink
    {
        protected ExecutionSession Session { get; }

        public ExecuteSinkHandler(ExecutionSession session)
        {
            Session = session;
        }

        protected override bool OnInit()
        {
            return true;
        }

        public override void Execute(IResultSetLoader loader, IExecutionContext context)
        {
            var metadata = Arguments.Procedure as Procedure;

            Debug.Assert(metadata != null, "metadata != null");
            string resourcePath = metadata.Path;
            Method method = metadata.Method;

            string query = null;
            var parameters = new Dictionary<string, object>(EqualityComparer<string>.Default);

            if (Arguments.Parameters.Args.Any())
            {
                for (var i = 0; i < Arguments.Procedure.Parameters.Count; i++)
                {
                    var parameterMetadata = Arguments.Procedure.Parameters[i] as Parameter;
                    Debug.Assert(parameterMetadata != null, "parameterMetadata != null");
                    var parameterValue = Arguments.Parameters[i];
                    var ctx = new ContextTranslatorInfo(context, parameterMetadata.Type);
                    switch (parameterMetadata.Location)
                    {
                        case Location.RequestBody:
                        {
                            var convertedParameterValue = ArgumentTranslator.Instance.Translate(parameterValue, ctx);
                            parameters.Add(parameterMetadata.Name, convertedParameterValue ?? "null");
                            break;
                        }
                        case Location.Query:
                        {
                            var convertedParameterValue = ArgumentTranslator.Instance.Translate(parameterValue, ctx);
                            if (convertedParameterValue is DBNull) continue;
                                query = query == null ? $"?{parameterValue.Name}={convertedParameterValue}" : $"&{parameterValue.Name}={convertedParameterValue}";
                            break;
                        }
                        case Location.UrlSegment:
                        {
                            var convertedParameterValue = ArgumentTranslator.Instance.Translate(parameterValue, ctx);
                            var temp = resourcePath.Replace(string.Concat('{', parameterMetadata.Name, '}'), convertedParameterValue.ToString());
                            resourcePath = temp;
                            break;
                        }
                        case Location.Unknown:
                            throw ConnectorExceptionFactory.Create(ConnectorExceptionType.UnexpectedException, nameof(parameterMetadata.Location));
                        default:
                            throw new ArgumentOutOfRangeException(nameof(parameterMetadata.Location));
                    }
                }
            }
            
            //Prepares request URL
            var resource = string.IsNullOrEmpty(query) ? resourcePath : string.Concat(resourcePath, query);
            //Performs request to Data Source
            var properties = (Properties)Session.Connector.ConnectorProperties;
            var client = new RestClient("https://www.googleapis.com/gmail/v1/")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(properties.AccessToken, properties.TokenType)
            };
            var request = new RestRequest(resource, method);
            if (parameters.Count > 1) request.AddParameter("application/json", JsonConvert.SerializeObject(parameters), ParameterType.RequestBody);
            // Execute the query and get the response object.
            var response = client.Execute(request);
            // Fetches data
            if (ResultColumns.Count > 0)
            {
                using (var tableLoader = loader.OpenTableResultLoader(ResultColumns))
                {
                    if ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 399)
                    {
                        var contextJsonInfo = new ContextJsonInfo();
                        contextJsonInfo.PropertyNameList.AddRange(ResultColumns);
                        using (var stream = new MemoryStream(response.RawBytes))
                        {
                            var jsonReader = new JsonResultReader();
                            var resultLoader = new TableResultLoader(tableLoader);
                            jsonReader.JsonRead += resultLoader.OnJsonRead;

                            jsonReader.Read(stream, contextJsonInfo);
                        }
                    }
                    else
                    {
                        ErrorUtils.GetErrorDetails(response.Content, out var code, out var message);
                        throw ConnectorExceptionFactory.Create(ConnectorExceptionType.DataSourceException, $"{code}: {message}");
                    }
                }
            }
            else
            {
                if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 399))
                {
                    ErrorUtils.GetErrorDetails(response.Content, out var code, out var message);
                    throw ConnectorExceptionFactory.Create(ConnectorExceptionType.DataSourceException, $"{code}: {message}");
                }
                loader.ReturnEmptyResult(null, 1, null); //Cannot evaluate number of affected rows.
            }
        }

    }
}