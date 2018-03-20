﻿using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swagger.Net.Dummy.Controllers;
using Swagger.Net.Dummy.SwaggerExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http.Description;

namespace Swagger.Net.Tests.Swagger
{
    [TestFixture]
    public class XmlCommentsTests : SwaggerTestBase
    {
        public XmlCommentsTests() : base("swagger/docs/{apiVersion}") { }

        [SetUp]
        public void SetUp()
        {
            SetUpAttributeRoutesFrom(typeof(XmlAnnotatedController).Assembly);
            SetUpDefaultRouteFor<XmlAnnotatedController>();
            SetUpDefaultRouteFor<BaseChildController>();
            SetUpHandler();
        }

        [Test]
        public void It_documents_operations_from_action_summary_and_remarks_tags_including_paramrefs()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var postOp = swagger["paths"]["/xmlannotated"]["post"];

            Assert.IsNotNull(postOp["summary"]);
            Assert.AreEqual("Registers a new Account based on {account}.", postOp["summary"].ToString());

            Assert.IsNotNull(postOp["description"]);
            Assert.AreEqual("Create an {Swagger.Net.Dummy.Controllers.Account} to access restricted resources", postOp["description"].ToString());
        }

        [Test]
        public void It_documents_operations_from_inherited_action()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var postOp = swagger["paths"]["/basechild"]["post"];

            Assert.IsNotNull(postOp["summary"]);
            Assert.AreEqual("Post a record.", postOp["summary"].ToString());
        }

        [Test]
        public void It_documents_operations_from_inherited_action_with_generic_parameter()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var postOp = swagger["paths"]["/basechild/{id}"]["put"];

            Assert.IsNotNull(postOp["summary"]);
            Assert.AreEqual("Put a record by the given key.", postOp["summary"].ToString());
        }

        [Test]
        public void It_documents_parameters_from_action_param_tags()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var accountParam = swagger["paths"]["/xmlannotated"]["post"]["parameters"][0];
            Assert.IsNotNull(accountParam["description"]);
            Assert.AreEqual("Details for the account to be created", accountParam["description"].ToString());

            var keywordsParam = swagger["paths"]["/xmlannotated"]["get"]["parameters"][0];
            Assert.IsNotNull(keywordsParam["description"]);
            Assert.AreEqual("List of search keywords", keywordsParam["description"].ToString());
        }

        [Test]
        public void It_documents_responses_from_action_response_tags()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var createResponses = swagger["paths"]["/xmlannotated"]["post"]["responses"];

            var expected = JObject.FromObject(new Dictionary<string, object>()
                {
                    {
                        "201", new
                        {
                            description = "{account} created",
                            schema = new
                            {
                                type = "integer",
                                format = "int32"
                            }
                        }
                    },
                    {
                        "400", new
                        {
                            description = "Username already in use"
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), createResponses.ToString());
        }

        [Test]
        public void It_documents_controllers()
        {
            var key = "XmlAnnotated";
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var tag = swagger.SelectToken($"$.tags[?(@.name == '{key}')]");
            Assert.IsNotNull(tag);
            Assert.IsNotNull(tag["description"]);
            Assert.IsTrue(tag["description"].ToString().Contains(key));
        }

        [Test]
        public void It_documents_schemas_from_type_summary_tags()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var accountSchema = swagger["definitions"]["Account"];

            Assert.IsNotNull(accountSchema["description"]);
            Assert.AreEqual("Account details", accountSchema["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_from_property_summary_tags()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var usernameProperty = swagger["definitions"]["Account"]["properties"]["Username"];
            Assert.IsNotNull(usernameProperty["description"]);
            Assert.AreEqual("Uniquely identifies the account", usernameProperty["description"].ToString());

            var passwordProperty = swagger["definitions"]["Account"]["properties"]["Password"];
            Assert.IsNotNull(passwordProperty["description"]);
            Assert.AreEqual("For authentication", passwordProperty["description"].ToString());

            var noteProperty = swagger["definitions"]["Account"]["properties"]["Note"];
            Assert.IsNotNull(noteProperty["description"]);
            Assert.AreEqual("Just a note", noteProperty["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_from_property_example_tags()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var passwordAccountId = swagger["definitions"]["Account"]["properties"]["AccountID"];
            Assert.IsNotNull(passwordAccountId["example"]);
            Assert.AreEqual("78312", passwordAccountId["example"].ToString());

            var usernameProperty = swagger["definitions"]["Account"]["properties"]["Username"];
            Assert.IsNotNull(usernameProperty["example"]);
            Assert.AreEqual("TestUser", usernameProperty["example"].ToString());

            var passwordProperty = swagger["definitions"]["Account"]["properties"]["Password"];
            Assert.IsNotNull(passwordProperty["example"]);
            Assert.AreEqual("TestPassword", passwordProperty["example"].ToString());

            var noteProperty = swagger["definitions"]["Account"]["properties"]["Note"];
            Assert.IsNotNull(noteProperty["example"]);
            Assert.AreEqual("HelloWorld", noteProperty["example"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_including_property_summary_tags_from_base_classes()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var usernameProperty = swagger["definitions"]["SubAccount"]["properties"]["Username"];
            Assert.IsNotNull(usernameProperty["description"]);
            Assert.AreEqual("Uniquely identifies the account", usernameProperty["description"].ToString());
        }

        [Test]
        public void It_documents_schema_default_parameters()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var parameters = swagger["paths"]["/xmlannotated/GetById"]["get"]["parameters"];
            Assert.IsNotNull(parameters);

            Assert.IsNotNull(parameters.First["required"]);
            Assert.AreEqual("False", parameters.First["required"].ToString());

            Assert.IsNotNull(parameters.First["default"]);
            Assert.AreEqual("123456", parameters.First["default"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_favoring_property_summary_tags_from_derived_vs_base_classes()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var usernameProperty = swagger["definitions"]["SubAccount"]["properties"]["AccountID"];
            Assert.IsNotNull(usernameProperty["description"]);
            Assert.AreEqual("The Account ID for SubAccounts should be 7 digits.", usernameProperty["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_from_summary_tags_of_complex_type_when_query_parameter_is_annotated_with_fromuri_attribute()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var parameters = swagger["paths"]["/xmlannotated/filter"]["get"]["parameters"];

            var qParam = parameters[0];
            Assert.IsNotNull(qParam["description"]);
            Assert.AreEqual("The search query on which to filter accounts", qParam["description"].ToString());

            var limitParam = parameters[1];
            Assert.IsNotNull(limitParam["description"]);
            Assert.AreEqual("The maximum number of accounts to return", limitParam["description"].ToString());

            var offsetParam = parameters[2];
            Assert.IsNotNull(offsetParam["description"]);
            Assert.AreEqual("Offset into the result", offsetParam["description"].ToString());
        }

        [Test]
        public void It_handles_actions_decorated_with_action_name()
        {
            Configuration.Routes.Clear();
            SetUpCustomRouteFor<XmlAnnotatedController>("XmlAnnotated/{id}/{action}");

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var operation = swagger["paths"]["/XmlAnnotated/{id}/put-on-hold"]["put"];
            Assert.IsNotNull(operation["summary"]);
            Assert.AreEqual("Prevents the account from being used", operation["summary"].ToString());
        }

        [Test]
        public void It_handles_actions_with_array_of_generic_parameters()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var operation = swagger["paths"]["/xmlannotated/{id}/metadata"]["put"];
            Assert.IsNotNull(operation["summary"]);
            Assert.AreEqual("Updates metadata associated with an account", operation["summary"].ToString());
        }

        [Test]
        public void It_handles_nested_class_properties()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var displayNameProperty = swagger["definitions"]["AccountPreferences"]["properties"]["DisplayName"];
            Assert.IsNotNull(displayNameProperty["description"]);
            Assert.AreEqual("Provide a display name to use instead of Username when signed in", displayNameProperty["description"].ToString());
        }

        [Test]
        public void It_handles_json_annotated_properties()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var marketingEmailsProperty = swagger["definitions"]["AccountPreferences"]["properties"]["allow-marketing-emails"];
            Assert.IsNotNull(marketingEmailsProperty["description"]);
            Assert.AreEqual("Flag to indicate if marketing emails may be sent", marketingEmailsProperty["description"].ToString());
        }

        [Test]
        public void It_handles_generic_types()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);

            var genericTypeSchema = swagger["definitions"]["Reward[String]"];
            Assert.NotNull(genericTypeSchema["description"]);
            Assert.AreEqual("A redeemable reward", genericTypeSchema["description"].ToString());

            var genericProperty = genericTypeSchema["properties"]["RewardType"];
            Assert.NotNull(genericProperty["description"]);
            Assert.AreEqual("The reward type", genericProperty["description"].ToString());
        }

        [Test]
        public void It_does_not_error_on_types_with_public_fields()
        {
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var valueProperty = swagger["definitions"]["Reward[String]"]["properties"]["value"];
            Assert.IsNotNull(valueProperty["description"]);
            Assert.AreEqual("The monetary value of the reward", valueProperty["description"].ToString());
        }

        [Test]
        public void It_does_not_load_bad_xml()
        {
            Exception ex = null;
            try
            {
                SetUpHandler(c => { c.IncludeXmlComments("BadXml.xml"); });
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex);
            // The exception message should contain something related to XML
            Assert.IsTrue(ex.Message.Contains("XML"));
        }

        [Test]
        public void It_does_not_load_bad_xml2()
        {
            Exception ex = null;
            try
            {
                SetUpHandler(c => { c.IncludeXmlComments(new string[] { "BadXml.xml"}); });
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex);
            // The exception message should contain something related to XML
            Assert.IsTrue(ex.Message.Contains("XML"));
        }

        [Test]
        public void It_loads_multiple_xml()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            SetUpHandler(c => { c.IncludeXmlComments(Directory.GetFiles(directory, "*.XML", SearchOption.AllDirectories)); });
            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            Assert.IsNotNull(swagger);
        }

        [Test]
        public void It_does_not_clear_previously_added_responses()
        {
            SetUpHandler(c =>
            {
                c.OperationFilter<InternalServerErrorResponseOperationFilter>();
            });

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var responsesProperty = swagger["paths"]["/xmlannotated"]["post"]["responses"];
            Assert.IsNotNull(responsesProperty["500"]);
        }

        [Test]
        public void It_does_not_error_on_multiple_calls()
        {
            SetUpHandler(c => { c.AllowCachingSwaggerDoc(); });
            JObject swagger;
            for (int i = 0; i < 100; i++)
            {
                swagger = GetContent<JObject>(TEMP_URI.DOCS);
                Assert.IsNotNull(swagger);
            }
        }

        [Test]
        public void It_loads_multiple_xml_comments()
        {
            SetUpHandler(c =>
            {
                c.DocumentFilter<ApplyDocumentVendorExtensions>();
            });

            var swagger = GetContent<JObject>(TEMP_URI.DOCS);
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions["SubAccount"]["description"]);
            Assert.IsNotNull(definitions["MockSequence"]["description"]);
        }

        private class ApplyDocumentVendorExtensions : IDocumentFilter
        {
            public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
            {
                schemaRegistry.GetOrRegister(typeof(MonthEnum));
                schemaRegistry.GetOrRegister(typeof(SubAccount));
                schemaRegistry.GetOrRegister(typeof(Moq.MockSequence));
            }
        }
    }
}