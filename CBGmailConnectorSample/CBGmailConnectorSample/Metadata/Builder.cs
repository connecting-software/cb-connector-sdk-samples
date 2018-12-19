using System;
using System.Collections.Generic;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using MG.CB.Metadata.MetaModel.Factories;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBGmailConnectorSample.Metadata
{
    // Models your connection target as a database.
    public class Builder : IBuilder
    {
        private const string SpecialParameter = "userId";
        private const string SpecialValue = "me";

        private readonly EagerMetaModelFactory _modelFactory = EagerMetaModelFactory.Instance;

        public Builder()
        {
            // Get the metadata from your connection target.
            // In case the connection target does not provide a means of accessing its metadata,
            // you should create JSON or XML representation for the metadata; and deserialize it.
            var discoveryService = new DiscoveryService();
            var request = discoveryService.Apis.GetRest("gmail", "v1");
            RestDescription = request.Execute();

            // Default schema for building qualified name
            DefaultSchema = _modelFactory.CreateSchema(RestDescription.Name);
            // Holds JSON SCHEMA
            Model = _modelFactory.CreateMetaModel(RestDescription.OwnerName, RestDescription.Name);
            // Default schema needs to be added to model
            Model.Add(DefaultSchema);
        }

        protected RestDescription RestDescription { get; }
        protected IMetaModel Model { get; }
        protected ISchema DefaultSchema { get; }
        
        public IMetaModel GetModel()
        {
            //Here you should build namespaces: tables, relationships, procedures, aggregate/arithmetic functions, and so on.
            //In this case, only procedures are created.
            BuildProcedures();
            return Model;
        }

        /// <summary> Builds the resources that belongs to the <see cref="RestDescription"/> object as procedures. </summary>
        protected void BuildProcedures()
        {
            BuildProcedures(RestDescription.Resources);
        }
        
        private void BuildProcedures(IDictionary<string, RestResource> resources, string root = null)
        {
            if(resources == null) return;
            foreach (var resource in resources)
            {
                if(resource.Value.Methods == null) continue;
                foreach (var method in resource.Value.Methods)
                {
                    var procedure = new Procedure($"{method.Key}{resource.Key}")
                    {
                        Tag = method.Value,
                        Method = method.Value.HttpMethod.Parse(),
                        Description = _modelFactory.CreateDescriptor(method.Value.Description)
                    };
                    var temp = string.IsNullOrEmpty(root) ? $"/{resource.Key}/{method.Value.Path}" : $"/{root}/{method.Value.Path}";
                    if (temp.Contains(SpecialParameter)) temp = temp.Replace(string.Concat('{', SpecialParameter, '}'), SpecialValue);
                    procedure.Path = temp;

                    DefaultSchema.Add(procedure);
                    BuildProcedure(procedure, method.Value);
                }
                
                BuildProcedures(resource.Value.Resources, string.IsNullOrEmpty(root) ? resource.Key : root);
            }
        }
        
        protected void BuildProcedure(IProcedure procedure, RestMethod method)
        {
            // Input Parameters
            // Ordinal position (starting at 1)
            int ordinal = 1;
            foreach (var methodParameter in method.Parameters)
            {
                var parameterName = methodParameter.Key;
                var parameterValue = methodParameter.Value;
                if(string.Compare(parameterName, SpecialParameter, StringComparison.OrdinalIgnoreCase) == 0) continue;

                // Resolve parameter type
                var type = TypeResolver.Instance.GetElementType(parameterValue, RestDescription);
                var isNullable = parameterValue.Required ?? true;
                var isReadOnly = parameterValue.ReadOnly__ ?? false;
                var defaultValue = parameterValue.Default__;

                //Resolve location
                Location location;
                switch (parameterValue.Location.ToLower())
                {
                    case "query":
                        location = Location.Query;
                        break;
                    case "path":
                        location = Location.UrlSegment;
                        break;
                    default:
                        location = Location.Unknown;
                        break;
                }

                var parameter = new Parameter(parameterName, type, isNullable, ordinal++)
                {
                    DefaultValue = defaultValue,
                    Location = location,
                    Description = _modelFactory.CreateDescriptor(parameterValue.Description),
                    IsReadOnly = isReadOnly
                };
                procedure.Add(parameter);
            }

            // Input Parameters
            // Request Body
            if (method.Request != null)
            {
                var reference = RestDescription.Schemas[method.Request.Ref__];
                foreach (var property in reference.Properties)
                {
                    var propertyName = property.Key;
                    var propertyValue = property.Value;
                    
                    // Resolve parameter type
                    var type = TypeResolver.Instance.GetElementType(propertyValue, RestDescription);
                    var isNullable = propertyValue.Required ?? true;
                    var isReadOnly = propertyValue.ReadOnly__ ?? false;
                    var defaultValue = propertyValue.Default__;

                    var parameter = new Parameter(propertyName, type, isNullable, ordinal++)
                    {
                        DefaultValue = defaultValue,
                        Location = Location.RequestBody,
                        Description = _modelFactory.CreateDescriptor(propertyValue.Description),
                        IsReadOnly = isReadOnly
                    };
                    procedure.Add(parameter);
                }
            }

            // Result Columns - the collection of columns that belong to the result set
            if (method.Response == null) return;
            var @ref = RestDescription.Schemas[method.Response.Ref__];
            // Ordinal position (starting at 1)
            ordinal = 1;
            foreach (var property in @ref.Properties)
            {
                var propertyName = property.Key;

                // Resolve parameter type
                var type = TypeResolver.Instance.GetElementType(property.Value, RestDescription);

                switch (type)
                {
                    case ComplexType complexType when complexType.StructuredType is ITableType tableType:
                    {
                        foreach (var namedElement in tableType.Elements)
                        {
                            var element = (IColumn)namedElement;
                            var column = new Column(element.Name, element.Type,element.IsNullable, ordinal++) { Path = $"/{propertyName}/{element.Name}" };
                            procedure.ResultColumns.Add(column);
                        }
                        break;
                    }
                    case CollectionType collectionType when collectionType.Type is ITableType tableType:
                    {
                        foreach (var namedElement in tableType.Elements)
                        {
                            var element = (IColumn)namedElement;
                            var column = new Column(element.Name, element.Type, element.IsNullable, ordinal++) { Path = $"/{propertyName}/{element.Name}" };
                            procedure.ResultColumns.Add(column);
                        }
                        break;
                    }
                    default:
                    {
                        var isNullable = property.Value.Required ?? true;
                        var column = new Column(propertyName, type, isNullable, ordinal++) { Path = $"/{propertyName}" };
                        procedure.ResultColumns.Add(column);
                        break;
                    }
                }

            }
        }

    }
}