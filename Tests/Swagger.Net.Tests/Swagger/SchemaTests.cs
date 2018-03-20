﻿using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swagger.Net.Dummy;
using Swagger.Net.Dummy.Controllers;
using Swagger.Net.Dummy.SwaggerExtensions;
using Swagger.Net.Dummy.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swagger.Net.Tests.Swagger
{
    [TestFixture]
    public class SchemaTests : SwaggerTestBase
    {
        public SchemaTests() : base("swagger/docs/{apiVersion}") { }

        [SetUp]
        public void SetUp()
        {
            // Default set-up
            SetUpHandler();
        }

        [Test]
        public void It_provides_definition_schemas_for_complex_types()
        {
            SetUpDefaultRouteFor<ProductsController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
            {
                Product = new
                {
                    properties = new
                    {
                        Id = new
                        {
                            readOnly = true,
                            type = "integer",
                            format = "int32",
                        },
                        Type = new
                        {
                            type = "integer",
                            format = "int32",
                            @enum = new[] { 2, 4 },
                        },
                        Description = new
                        {
                            type = "string"
                        },
                        UnitPrice = new
                        {
                            type = "number",
                            format = "double",
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"Product\" }"),
                    type = "object"
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_provides_object_schemas_for_dictionary_types_with_enum_keys()
        {
            SetUpCustomRouteFor<DictionaryTypesController>("term-definitions");

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var schema = swagger["paths"]["/term-definitions"]["get"]["responses"]["200"]["schema"];

            var expected = JObject.FromObject(new
            {
                properties = new
                {
                    TermA = new
                    {
                        type = "string"
                    },
                    TermB = new
                    {
                        type = "string"
                    }
                },
                type = "object"
            });

            Assert.IsNotNull(schema);
            Assert.AreEqual(expected.ToString(), schema.ToString());
        }

        [Test]
        public void It_provides_validation_properties_for_metadata_annotated_types()
        {
            SetUpDefaultRouteFor<MetadataAnnotatedTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var get = swagger["paths"]["/metadataannotatedtypes"]["get"];
            Assert.IsNotNull(get);
            var param = get["parameters"].First();
            Assert.IsNotNull(param["pattern"]);
            Assert.IsNotNull(param["x-example"]);

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
            {
                PaymentWithMetadata = new
                {
                    required = new[] { "Amount", "CardNumber", "ExpMonth", "ExpYear" },
                    properties = new
                    {
                        Amount = new
                        {
                            type = "number",
                            format = "double",
                        },
                        CardNumber = new
                        {
                            type = "string",
                            pattern = "^[3-6]?\\d{12,15}$",
                        },
                        ExpMonth = new
                        {
                            type = "integer",
                            format = "int32",
                            maximum = 12,
                            minimum = 1,
                        },
                        ExpYear = new
                        {
                            type = "integer",
                            format = "int32",
                            maximum = 99,
                            minimum = 14,
                        },
                        Note = new
                        {
                            type = "string",
                            @default = "HelloWorld",
                            maxLength = 500,
                            minLength = 10,
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"PaymentWithMetadata\" }"),
                    type = "object",
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_provides_validation_properties_for_annotated_types()
        {
            SetUpDefaultRouteFor<AnnotatedTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
            {
                Payment = new
                {
                    required = new[] { "Amount", "CardNumber", "ExpMonth", "ExpYear", "Tax" },
                    properties = new
                    {
                        Amount = new
                        {
                            type = "number",
                            format = "double",
                        },
                        CardNumber = new
                        {
                            type = "string",
                            pattern = "^[3-6]?\\d{12,15}$",
                        },
                        ExpMonth = new
                        {
                            description = "Credit card expiration Month",
                            example = "6",
                            type = "integer",
                            format = "int32",
                            maximum = 12,
                            minimum = 1,
                        },
                        ExpYear = new
                        {
                            description = "Credit card expiration Year",
                            example = "96",
                            type = "integer",
                            format = "int32",
                            maximum = 99,
                            minimum = 14,
                        },
                        Note = new
                        {
                            type = "string",
                            maxLength = 500,
                            minLength = 10,
                        },
                        guid = new
                        {
                            example = "00000000-0000-0000-0000-000000000000",
                            type = "string",
                            format = "uuid",
                        },
                        Detail = new
                        {
                            type = "string",
                            maxLength = 100,
                            minLength = 2,
                        },
                        Tax = new
                        {
                            type = "number",
                            format = "double",
                            maximum = 32.9,
                            minimum = 1.1,
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"Payment\" }"),
                    type = "object"
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_includes_inherited_properties_for_sub_types()
        {
            SetUpDefaultRouteFor<PolymorphicTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
            {
                Elephant = new
                {
                    properties = new
                    {
                        TrunkLength = new
                        {
                            type = "integer",
                            format = "int32"
                        },
                        HairColor = new
                        {
                            type = "string"
                        },
                        Type = new
                        {
                            type = "string"
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"Elephant\" }"),
                    type = "object"
                },
                Animal = new
                {
                    properties = new
                    {
                        Type = new
                        {
                            type = "string"
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"Animal\" }"),
                    type = "object"
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_omits_indexer_properties()
        {
            SetUpDefaultRouteFor<IndexerTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var definitions = swagger["definitions"];

            var expected = JObject.FromObject(new
            {
                Lookup = new
                {
                    properties = new
                    {
                        TotalEntries = new
                        {
                            type = "integer",
                            format = "int32"
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"Lookup\" }"),
                    type = "object"
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_honors_json_annotated_attributes()
        {
            SetUpDefaultRouteFor<JsonAnnotatedTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
            {
                JsonRequest = new
                {
                    properties = new
                    {
                        foobar = new
                        {
                            type = "string"
                        },
                        Category = new
                        {
                            type = "string",
                            @enum = new[] { "A", "B" }
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"JsonRequest\" }"),
                    type = "object"
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_honors_json_string_enum_converter_configured_globally()
        {
            Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(
                new StringEnumConverter { CamelCaseText = true });
            SetUpDefaultRouteFor<ProductsController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var typeSchema = swagger["definitions"]["Product"]["properties"]["Type"];

            var expected = JObject.FromObject(new
            {
                type = "string",
                @enum = new[] { "publication", "album" },
            });
            Assert.AreEqual(expected.ToString(), typeSchema.ToString());
        }

        [Test]
        public void It_exposes_config_to_map_a_type_to_an_explicit_schema()
        {
            SetUpDefaultRouteFor<ProductsController>();
            SetUpHandler(c => c.MapType<ProductType>(() => new Schema
            {
                type = "integer",
                format = "int32",
                maximum = 2,
                minimum = 1
            }));

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var parameter = swagger["paths"]["/products"]["get"]["parameters"][0];

            var expected = JObject.FromObject(new
            {
                name = "type",
                @in = "query",
                required = true,
                type = "integer",
                format = "int32",
                maximum = 2,
                minimum = 1
            });
            Assert.AreEqual(expected.ToString(), parameter.ToString());
        }

        [Test]
        public void It_exposes_config_to_post_modify_schemas_for_mapped_types()
        {
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.MapType<Guid>(() => new Schema { type = "string", format = "guid" }); // map format to guid instead of uuid
                c.SchemaFilter<ApplySchemaVendorExtensions>();

            });

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var operation = swagger["paths"]["/PrimitiveTypes/EchoGuid"]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveTypesController).GetMethod("EchoGuid");
            Assert.AreEqual(typeof(Guid), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(typeof(Guid), method.ReturnType);

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "value" },
                { "in", "query" },
                { "required", true },
                { "type", "string" },
                { "format", "guid" },
                { "x-type-dotnet", "System.Guid" },
                { "x-nullable", false }
            };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>()
            {
                { "type", "string" },
                { "format", "guid" },
                { "x-type-dotnet", "System.Guid" },
                { "x-nullable", false }
            };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [TestCase("EchoBoolean", typeof(bool), "boolean", null, "System.Boolean", false)]
        [TestCase("EchoByte", typeof(byte), "integer", "int32", "System.Byte", false)]
        [TestCase("EchoSByte", typeof(sbyte), "integer", "int32", "System.SByte", false)]
        [TestCase("EchoInt16", typeof(short), "integer", "int32", "System.Int16", false)]
        [TestCase("EchoUInt16", typeof(ushort), "integer", "int32", "System.UInt16", false)]
        [TestCase("EchoInt32", typeof(int), "integer", "int32", "System.Int32", false)]
        [TestCase("EchoUInt32", typeof(uint), "integer", "int32", "System.UInt32", false)]
        [TestCase("EchoInt64", typeof(long), "integer", "int64", "System.Int64", false)]
        [TestCase("EchoUInt64", typeof(ulong), "integer", "int64", "System.UInt64", false)]
        [TestCase("EchoSingle", typeof(float), "number", "float", "System.Single", false)]
        [TestCase("EchoDouble", typeof(double), "number", "double", "System.Double", false)]
        [TestCase("EchoDecimal", typeof(decimal), "number", "double", "System.Decimal", false)]
        [TestCase("EchoDateTime", typeof(DateTime), "string", "date-time", "System.DateTime", false)]
        [TestCase("EchoDateTimeOffset", typeof(DateTimeOffset), "string", "date-time", "System.DateTimeOffset", false)]
        [TestCase("EchoTimeSpan", typeof(TimeSpan), "string", null, "System.TimeSpan", false)]
        [TestCase("EchoGuid", typeof(Guid), "string", "uuid", "System.Guid", false)]
        [TestCase("EchoEnum", typeof(PrimitiveEnum), "integer", "int32", "Swagger.Net.Dummy.Types.PrimitiveEnum", false)]
        [TestCase("EchoEnum", typeof(PrimitiveEnum), "string", null, "Swagger.Net.Dummy.Types.PrimitiveEnum", false)]
        [TestCase("EchoChar", typeof(char), "string", null, "System.Char", false)]
        [TestCase("EchoNullableBoolean", typeof(bool?), "boolean", null, "System.Boolean", true)]
        [TestCase("EchoNullableByte", typeof(byte?), "integer", "int32", "System.Byte", true)]
        [TestCase("EchoNullableSByte", typeof(sbyte?), "integer", "int32", "System.SByte", true)]
        [TestCase("EchoNullableInt16", typeof(short?), "integer", "int32", "System.Int16", true)]
        [TestCase("EchoNullableUInt16", typeof(ushort?), "integer", "int32", "System.UInt16", true)]
        [TestCase("EchoNullableInt32", typeof(int?), "integer", "int32", "System.Int32", true)]
        [TestCase("EchoNullableUInt32", typeof(uint?), "integer", "int32", "System.UInt32", true)]
        [TestCase("EchoNullableInt64", typeof(long?), "integer", "int64", "System.Int64", true)]
        [TestCase("EchoNullableUInt64", typeof(ulong?), "integer", "int64", "System.UInt64", true)]
        [TestCase("EchoNullableSingle", typeof(float?), "number", "float", "System.Single", true)]
        [TestCase("EchoNullableDouble", typeof(double?), "number", "double", "System.Double", true)]
        [TestCase("EchoNullableDecimal", typeof(decimal?), "number", "double", "System.Decimal", true)]
        [TestCase("EchoNullableDateTime", typeof(DateTime?), "string", "date-time", "System.DateTime", true)]
        [TestCase("EchoNullableDateTimeOffset", typeof(DateTimeOffset?), "string", "date-time", "System.DateTimeOffset", true)]
        [TestCase("EchoNullableTimeSpan", typeof(TimeSpan?), "string", null, "System.TimeSpan", true)]
        [TestCase("EchoNullableGuid", typeof(Guid?), "string", "uuid", "System.Guid", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "integer", "int32", "Swagger.Net.Dummy.Types.PrimitiveEnum", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "string", null, "Swagger.Net.Dummy.Types.PrimitiveEnum", true)]
        [TestCase("EchoNullableChar", typeof(char?), "string", null, "System.Char", true)]
        [TestCase("EchoString", typeof(string), "string", null, "System.String", true)]
        public void It_exposes_config_to_post_modify_schemas_for_primitives(string action, Type dotNetType, string type, string format, string xtypeDotNet, bool xnullable)
        {
            var underlyingDotNetType = Nullable.GetUnderlyingType(dotNetType) ?? dotNetType;
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                if (underlyingDotNetType.IsEnum && type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter<ApplySchemaVendorExtensions>();
            });

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var operation = swagger["paths"]["/PrimitiveTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveTypesController).GetMethod(action);
            Assert.AreEqual(dotNetType, method.GetParameters()[0].ParameterType);
            Assert.AreEqual(dotNetType, method.ReturnType);

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "value" },
                { "in", "query" },
                { "required", true },
                { "type", type }
            };
            if (format != null)
            {
                expectedParameter.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedParameter.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedParameter.Add("x-type-dotnet", xtypeDotNet);
            expectedParameter.Add("x-nullable", xnullable);
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>();
            if (response?["example"] != null)
            {
                response["example"] = "";
                expectedResponse.Add("example", "");
            }
            expectedResponse.Add("type", type);
            if (format != null)
            {
                expectedResponse.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedResponse.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedResponse.Add("x-type-dotnet", xtypeDotNet);
            expectedResponse.Add("x-nullable", xnullable);
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [TestCase("EchoBoolean", typeof(bool), "boolean", null, "System.Boolean", false)]
        [TestCase("EchoByte", typeof(byte), "string", "byte", "System.Byte[]", true)] // Special case
        [TestCase("EchoSByte", typeof(sbyte), "integer", "int32", "System.SByte", false)]
        [TestCase("EchoInt16", typeof(short), "integer", "int32", "System.Int16", false)]
        [TestCase("EchoUInt16", typeof(ushort), "integer", "int32", "System.UInt16", false)]
        [TestCase("EchoInt32", typeof(int), "integer", "int32", "System.Int32", false)]
        [TestCase("EchoUInt32", typeof(uint), "integer", "int32", "System.UInt32", false)]
        [TestCase("EchoInt64", typeof(long), "integer", "int64", "System.Int64", false)]
        [TestCase("EchoUInt64", typeof(ulong), "integer", "int64", "System.UInt64", false)]
        [TestCase("EchoSingle", typeof(float), "number", "float", "System.Single", false)]
        [TestCase("EchoDouble", typeof(double), "number", "double", "System.Double", false)]
        [TestCase("EchoDecimal", typeof(decimal), "number", "double", "System.Decimal", false)]
        [TestCase("EchoDateTime", typeof(DateTime), "string", "date-time", "System.DateTime", false)]
        [TestCase("EchoDateTimeOffset", typeof(DateTimeOffset), "string", "date-time", "System.DateTimeOffset", false)]
        [TestCase("EchoTimeSpan", typeof(TimeSpan), "string", null, "System.TimeSpan", false)]
        [TestCase("EchoGuid", typeof(Guid), "string", "uuid", "System.Guid", false)]
        [TestCase("EchoChar", typeof(char), "string", null, "System.Char", false)]
        [TestCase("EchoNullableBoolean", typeof(bool?), "boolean", null, "System.Boolean", true)]
        [TestCase("EchoNullableByte", typeof(byte?), "integer", "int32", "System.Byte", true)]
        [TestCase("EchoNullableSByte", typeof(sbyte?), "integer", "int32", "System.SByte", true)]
        [TestCase("EchoNullableInt16", typeof(short?), "integer", "int32", "System.Int16", true)]
        [TestCase("EchoNullableUInt16", typeof(ushort?), "integer", "int32", "System.UInt16", true)]
        [TestCase("EchoNullableInt32", typeof(int?), "integer", "int32", "System.Int32", true)]
        [TestCase("EchoNullableUInt32", typeof(uint?), "integer", "int32", "System.UInt32", true)]
        [TestCase("EchoNullableInt64", typeof(long?), "integer", "int64", "System.Int64", true)]
        [TestCase("EchoNullableUInt64", typeof(ulong?), "integer", "int64", "System.UInt64", true)]
        [TestCase("EchoNullableSingle", typeof(float?), "number", "float", "System.Single", true)]
        [TestCase("EchoNullableDouble", typeof(double?), "number", "double", "System.Double", true)]
        [TestCase("EchoNullableDecimal", typeof(decimal?), "number", "double", "System.Decimal", true)]
        [TestCase("EchoNullableDateTime", typeof(DateTime?), "string", "date-time", "System.DateTime", true)]
        [TestCase("EchoNullableDateTimeOffset", typeof(DateTimeOffset?), "string", "date-time", "System.DateTimeOffset", true)]
        [TestCase("EchoNullableTimeSpan", typeof(TimeSpan?), "string", null, "System.TimeSpan", true)]
        [TestCase("EchoNullableGuid", typeof(Guid?), "string", "uuid", "System.Guid", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "integer", "int32", "Swagger.Net.Dummy.Types.PrimitiveEnum", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "string", null, "Swagger.Net.Dummy.Types.PrimitiveEnum", true)]
        [TestCase("EchoNullableChar", typeof(char?), "string", null, "System.Char", true)]
        [TestCase("EchoString", typeof(string), "string", null, "System.String", true)]
        public void It_exposes_config_to_post_modify_schemas_for_primitive_arrays(string action, Type dotNetType, string type, string format, string xtypeDotNet, bool xnullable)
        {
            var underlyingDotNetType = Nullable.GetUnderlyingType(dotNetType) ?? dotNetType;
            SetUpCustomRouteFor<PrimitiveArrayTypesController>("PrimitiveArrayTypes/{action}");
            SetUpHandler(c =>
            {
                if (underlyingDotNetType.IsEnum && type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter<ApplySchemaVendorExtensions>();
            });

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var operation = swagger["paths"]["/PrimitiveArrayTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];
            var hasExample = parameter?["schema"]?["items"]?["example"] != null;
            if (hasExample)
            {
                parameter["schema"]["items"]["example"] = "";
                response["items"]["example"] = "";
            }

            var method = typeof(PrimitiveArrayTypesController).GetMethod(action);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.ReturnType);

            var expectedParameterItems = new Dictionary<string, object>();
            if (hasExample)
                expectedParameterItems.Add("example", "");
            expectedParameterItems.Add("type", type);
            if (format != null)
            {
                expectedParameterItems.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedParameterItems.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedParameterItems.Add("x-type-dotnet", xtypeDotNet);
            expectedParameterItems.Add("x-nullable", xnullable);
            var expectedParameter = (format == "byte") // Special case
                ? JObject.FromObject(new
                {
                    name = "value",
                    @in = "body",
                    required = true,
                    schema = expectedParameterItems
                })
                : JObject.FromObject(new
                {
                    name = "value",
                    @in = "body",
                    required = true,
                    schema = new
                    {
                        items = expectedParameterItems,
                        type = "array"
                    }
                });
            Assert.AreEqual(expectedParameter.ToString(), parameter.ToString());

            var expectedResponseItems = new Dictionary<string, object>();
            if (hasExample)
                expectedResponseItems.Add("example", "");
            expectedResponseItems.Add("type", type);
            if (format != null)
            {
                expectedResponseItems.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedResponseItems.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedResponseItems.Add("x-type-dotnet", xtypeDotNet);
            expectedResponseItems.Add("x-nullable", xnullable);
            var expectedResponse = (format == "byte") // Special case
                ? JObject.FromObject(expectedResponseItems)
                : JObject.FromObject(new
                {
                    items = expectedResponseItems,
                    type = "array"
                });
            Assert.AreEqual(expectedResponse.ToString(), response.ToString());
        }

        [TestCase("EchoEnum", typeof(PrimitiveEnum), "integer", "int32", "Swagger.Net.Dummy.Types.PrimitiveEnum", false)]
        [TestCase("EchoEnum", typeof(PrimitiveEnum), "string", null, "Swagger.Net.Dummy.Types.PrimitiveEnum", false)]
        public void It_exposes_config_to_post_modify_schemas_for_primitive_enum_arrays(string action, Type dotNetType, string type, string format, string xtypeDotNet, bool xnullable)
        {
            var underlyingDotNetType = Nullable.GetUnderlyingType(dotNetType) ?? dotNetType;
            SetUpCustomRouteFor<PrimitiveArrayTypesController>("PrimitiveArrayTypes/{action}");
            SetUpHandler(c =>
            {
                if (underlyingDotNetType.IsEnum && type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter<ApplySchemaVendorExtensions>();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveArrayTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveArrayTypesController).GetMethod(action);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.ReturnType);

            var expectedParameterItems = new Dictionary<string, object>();
            expectedParameterItems.Add("type", type);
            if (format != null)
            {
                expectedParameterItems.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedParameterItems.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedParameterItems.Add("x-type-dotnet", xtypeDotNet);
            expectedParameterItems.Add("x-nullable", xnullable);
            var expectedParameter = (format == "byte") // Special case
                ? JObject.FromObject(new
                {
                    name = "value",
                    @in = "body",
                    required = true,
                    schema = expectedParameterItems
                })
                : JObject.FromObject(new
                {
                    name = "value",
                    @in = "body",
                    required = true,
                    schema = new
                    {
                        items = expectedParameterItems,
                        xml = JObject.Parse("{ \"name\": \"PrimitiveEnum\", \"wrapped\": true }"),
                        type = "array"
                    }
                });
            Assert.AreEqual(expectedParameter.ToString(), parameter.ToString());

            var expectedResponseItems = new Dictionary<string, object>();
            expectedResponseItems.Add("type", type);
            if (format != null)
            {
                expectedResponseItems.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedResponseItems.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedResponseItems.Add("x-type-dotnet", xtypeDotNet);
            expectedResponseItems.Add("x-nullable", xnullable);
            var expectedResponse = (format == "byte") // Special case
                ? JObject.FromObject(expectedResponseItems)
                : JObject.FromObject(new
                {
                    items = expectedResponseItems,
                    xml = JObject.Parse("{ \"name\": \"PrimitiveEnum\", \"wrapped\": true }"),
                    type = "array"
                });
            Assert.AreEqual(expectedResponse.ToString(), response.ToString());
        }

        [Test]
        public void It_exposes_config_to_ignore_all_properties_that_are_obsolete()
        {
            SetUpDefaultRouteFor<ObsoletePropertiesController>();
            SetUpHandler(c => c.IgnoreObsoleteProperties());

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var calendarProps = swagger["definitions"]["Event"]["properties"];
            var expectedProps = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "Name", new { type = "string" }
                    }
                });

            Assert.AreEqual(expectedProps.ToString(), calendarProps.ToString());
        }

        [Test]
        public void It_exposes_config_to_workaround_multiple_types_with_the_same_class_name()
        {
            SetUpDefaultRouteFor<ConflictingTypesController>();
            SetUpHandler(c => c.UseFullTypeNameInSchemaIds());

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var defintitions = swagger["definitions"];

            Assert.AreEqual(2, defintitions.Count());
        }

        [Test]
        public void It_exposes_config_to_choose_schema_id()
        {
            SetUpDefaultRouteFor<ProductsController>();
            SetUpHandler(c => c.SchemaId(t => "my custom name"));

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var defintitions = swagger["definitions"];

            Assert.IsNotNull(defintitions["my custom name"]);
        }

        [Test]
        public void It_exposes_config_to_modify_schema_ids()
        {
            SetUpDefaultRouteFor<ConflictingTypesController>();
            // We have to know the default implementation of FriendlyId before we can modify it's output.
            SetUpHandler(c => { c.SchemaId(t => t.FriendlyId(true).Replace("Swagger.Net.Dummy.Controllers.", String.Empty)); });

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var defintitions = swagger["definitions"];

            Assert.IsNotNull(defintitions["Requests.Blog"]);
        }

        [Test]
        public void It_handles_nested_types()
        {
            SetUpDefaultRouteFor<NestedTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
            {
                Order = new
                {
                    properties = new
                    {
                        LineItems = new
                        {
                            items = JObject.Parse("{ $ref: \"#/definitions/LineItem\" }"),
                            xml = JObject.Parse("{ \"name\": \"LineItem\", \"wrapped\": true }"),
                            type = "array"
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"Order\" }"),
                    type = "object"
                },
                LineItem = new
                {
                    properties = new
                    {
                        ProductId = new
                        {
                            type = "integer",
                            format = "int32"
                        },
                        Quantity = new
                        {
                            type = "integer",
                            format = "int32"
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"LineItem\" }"),
                    type = "object"
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_self_referencing_types()
        {
            SetUpDefaultRouteFor<SelfReferencingTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
            {
                Component = new
                {
                    properties = new
                    {
                        Name = new
                        {
                            type = "string"
                        },
                        SubComponents = new
                        {
                            items = JObject.Parse("{ $ref: \"#/definitions/Component\" }"),
                            xml = JObject.Parse("{ \"name\": \"Component\", \"wrapped\": true }"),
                            type = "array"
                        }
                    },
                    xml = JObject.Parse("{ \"name\": \"Component\" }"),
                    type = "object",
                },
                // Breaks current swagger-ui
                //ListOfSelf = new
                //{
                //    type = "array",
                //    items = JObject.Parse("{ $ref: \"ListOfSelf\" }")
                //},
                DictionaryOfSelf = new
                {
                    additionalProperties = JObject.Parse("{ $ref: \"#/definitions/DictionaryOfSelf\" }"),
                    type = "object"
                }
            });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_two_dimensional_arrays()
        {
            SetUpDefaultRouteFor<TwoDimensionalArraysController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var schema = swagger["paths"]["/twodimensionalarrays"]["post"]["parameters"][0]["schema"];

            var expected = JObject.FromObject(new
            {
                items = new
                {
                    items = new
                    {
                        type = "integer",
                        format = "int32"
                    },
                    type = "array",
                },
                type = "array"
            });
            Assert.AreEqual(expected.ToString(), schema.ToString());
        }

        [Test]
        public void It_handles_HashSet_arrays()
        {
            SetUpDefaultRouteFor<TwoDimensionalArraysController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var schema = swagger["paths"]["/twodimensionalarrays"]["get"]["parameters"][0];

            var expected = JObject.FromObject(new
            {
                name = "matrix",
                @in = "query",
                required = true,
                items = new
                {
                    type = "string"
                },
                collectionFormat = "multi",
                type = "array",
                uniqueItems = true
            });
            Assert.AreEqual(expected.ToString(), schema.ToString());
        }

        [Test]
        public void It_handles_dynamic_types()
        {
            SetUpDefaultRouteFor<DynamicTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "DynamicObjectSubType", new
                        {
                            properties = new
                            {
                                Id = new
                                {
                                    type = "number",
                                    format = "double",
                                    maximum = 9.9,
                                    minimum = 1.1
                                },
                                Name = new
                                {
                                    type = "string"
                                }
                            },
                            xml = JObject.Parse("{ \"name\": \"DynamicObjectSubType\" }"),
                            type = "object"
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_nullable_types()
        {
            SetUpDefaultRouteFor<NullableTypesController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var definitions = swagger["definitions"];

            var expected = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "Contact", new
                        {
                            properties = new
                            {
                                Name = new
                                {
                                    type = "string"
                                },
                                Phone = new
                                {
                                    type = "integer",
                                    format = "int32"
                                }
                            },
                            xml = JObject.Parse("{ \"name\": \"Contact\" }"),
                            type = "object"
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_recursion_if_called_again_within_a_filter()
        {
            SetUpCustomRouteFor<ProductsController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.SchemaFilter<RecursiveCallSchemaFilter>();
                c.ModelFilter<ModFilter>();
            });

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
        }

        [Test]
        public void It_errors_on_multiple_types_with_the_same_class_name()
        {
            SetUpDefaultRouteFor<ConflictingTypesController>();

            Assert.Throws<InvalidOperationException>(() => GetContent<JObject>(TEMP_URI.DOCS));
        }

        [Test]
        public void It_always_marks_path_parameters_as_required()
        {
            SetUpDefaultRouteFor<PathRequiredController>();

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var required = (bool)swagger["paths"]["/pathrequired/{id}"]["get"]["parameters"][0]["required"];

            Assert.IsTrue(required);
        }

        [Test]
        public void It_marks_required_properties_as_required()
        {
            SetUpDefaultRouteFor<PathRequiredController>();

            var swagger = GetContent<SwaggerDocument>(TEMP_URI.DOCS);

            Assert.IsNotNull(swagger.definitions["ComplexObject1"].required);
        }

        [Test]
        public void It_marks_required_fields_as_required()
        {
            SetUpDefaultRouteFor<PathRequiredController>();

            var swagger = GetContent<SwaggerDocument>(TEMP_URI.DOCS);

            Assert.IsNotNull(swagger.definitions["ComplexObject2"].required);
        }
    }
}
