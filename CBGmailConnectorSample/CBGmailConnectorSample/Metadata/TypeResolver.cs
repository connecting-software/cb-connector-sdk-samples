using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Discovery.v1.Data;
using MG.CB.Metadata.MetaModel.Factories;
using MG.CB.Metadata.MetaModel.Interfaces;

namespace CBGmailConnectorSample.Metadata
{
    ///<summary>Provides methods to get <see cref="IDataType"/> from <see cref="JsonSchema"/> definition.</summary> 
    public class TypeResolver
    {
        private readonly EagerMetaModelFactory _modelFactory = EagerMetaModelFactory.Instance;
        private readonly PrimitiveTypesFactory _typesFactory = PrimitiveTypesFactory.Instance;

        #region Singleton - Usage: TypeResolver.Instance

        private TypeResolver()
        {
        }

        /// <summary> Gets a single instance of <see cref="TypeResolver"/> class. </summary>
        public static TypeResolver Instance { get; } = new TypeResolver();

        #endregion

        /// <summary> Gets the primitive type definition from <see cref="JsonSchema"/> definition.  </summary>
        /// <param name="obj">The <see cref="JsonSchema"/> definition.</param>
        /// <param name="restDescription">The metadata description.</param>
        /// <param name="visitedNodes">A collection of <see cref="IComplexType"/>.</param>
        /// <returns>Definition of the primitive type.</returns>
        /// <exception cref="ArgumentException">obj</exception>
        public IPrimitiveType GetElementType(JsonSchema obj, RestDescription restDescription, IList<IComplexType> visitedNodes = null)
        {
            //Currently, only primitive types can be defined for properties.
            if(visitedNodes == null) visitedNodes = new List<IComplexType>();
            switch (obj.Type)
            {
                case "string" when obj.Enum__ != null:
                    var enumType = new EnumType();
                    foreach (var member in obj.Enum__) enumType.Members.Add(member);
                    return enumType;
                case "string":

                    return _typesFactory.CreateStringType(int.MaxValue);
                case "boolean":
                    return _typesFactory.CreateBooleanType();
                case "integer":
                    return _typesFactory.CreateInt32Type();
                case null when obj.Ref__ != null:
                {
                    var @ref = restDescription.Schemas[obj.Ref__];
                    var tableType = visitedNodes.SingleOrDefault(d => d.Name == @ref.Id);
                    if (tableType != null) return new ComplexType(tableType);

                    tableType = _modelFactory.CreateTableType(@ref.Id);
                    visitedNodes.Add(tableType);
                    // Ordinal position (starting at 1)
                    int ordinal = 1;
                    foreach (var property in @ref.Properties)
                    {
                        var propertyName = property.Key;
                        // Resolve parameter type
                        var type = GetElementType(property.Value, restDescription, visitedNodes);
                        var isNullable = property.Value.Required ?? true;

                        var column = _modelFactory.CreateColumn(propertyName, type, isNullable, ordinal++, false, false,
                            false, false);
                        tableType.Add(column);
                    }
                    return new ComplexType(tableType);
                }
                case "array" when obj.Items.Ref__ != null:
                {
                    var @ref = restDescription.Schemas[obj.Items.Ref__];
                    var tableType = visitedNodes.SingleOrDefault(d => d.Name == @ref.Id);
                    if (tableType != null) return new ComplexType(tableType);

                    tableType = _modelFactory.CreateTableType(@ref.Id);
                    visitedNodes.Add(tableType);
                    // Ordinal position (starting at 1)
                    int ordinal = 1;
                    foreach (var property in @ref.Properties)
                    {
                        var propertyName = property.Key;

                        // Resolve parameter type
                        var type = GetElementType(property.Value, restDescription, visitedNodes);
                        var isNullable = property.Value.Required ?? true;

                        var column = _modelFactory.CreateColumn(propertyName, type, isNullable, ordinal++, false, false,
                            false, false);
                        tableType.Add(column);
                    }

                    return new CollectionType(tableType);
                }
                case "array":
                    return new CollectionType(GetElementType(obj.Items, restDescription, visitedNodes));
                default: throw new ArgumentException(nameof(obj));
            }
        }

    }
}