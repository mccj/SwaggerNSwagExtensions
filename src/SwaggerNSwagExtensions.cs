//最后修改 2020-03-13

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Namotion.Reflection;
using Newtonsoft.Json;
using NSwag;
using NSwag.Annotations;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Collections.Generic;
using System.Linq;

namespace SwaggerExtensions
{
    public static class SwaggerNSwagExtensions
    {
        public const string DefaultSecurityName = "DefaultAuth";
        public static IServiceCollection AddNSwagSwagger(this IServiceCollection services, NSwagConfig config = null)
        {
            services.AddSingleton<IOperationProcessor>(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor(config?.OperationSecurity?.SecurityName ?? DefaultSecurityName));//授权控制
            services.AddSingleton<IDocumentProcessor, DocumentControllerTagsProcessor>();//控制器注释
            services
                // Register an OpenAPI 3.0 document generator
                //.AddOpenApiDocument((document, sp) =>
                //{
                //    _settings(document, config, "openapi/");
                //    //document.DocumentName = "openapi/" + document.Version;
                //    //document.ApiGroupNames = new[] { "v1" };
                //})
                // Register a Swagger 2.0 document generator
                .AddSwaggerDocument((document, sp) =>
                {
                    _settings(document, config, "swagger/");
                    //document.DocumentName = "swagger/" + document.Version;
                })
            ;
            return services;
        }
        public static IApplicationBuilder UseNSwagSwaggerUI(this IApplicationBuilder app)
        {
            app.UseOpenApi(config =>
            {
                //if (!string.IsNullOrWhiteSpace(path))
                //    config.Path = path;

                config.PostProcess = (document, request) =>
                {
                    //document.Operations.FirstOrDefault().Operation.OperationId;
                    if (request.Headers.ContainsKey("X-External-Host"))
                    {
                        // Change document server settings to public
                        document.Host = request.Headers["X-External-Host"].First();
                        document.BasePath = request.Headers["X-External-Path"].First();
                    }
                };
            });

            var transformToExternalPath = new System.Func<string, Microsoft.AspNetCore.Http.HttpRequest, string>((internalUiRoute, request) =>
            {
                // The header X-External-Path is set in the nginx.conf file
                var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
                return externalPath + internalUiRoute;
            });

            app.UseSwaggerUi3(config =>
            {
                //if (!string.IsNullOrWhiteSpace(path))
                //    config.Path = path;
                config.TransformToExternalPath = transformToExternalPath;


                config.ValidateSpecification = true;
                //config.EnableTryItOut = false;//是否显示测试按钮
                //config.CustomHeadContent = "";

                //config.OAuth2Client = new NSwag.AspNetCore.OAuth2ClientSettings() { 
                //};
            });

            app.UseReDoc(config =>
            {
                //if (!string.IsNullOrWhiteSpace(path))
                //    config.Path = path;

                config.Path = "/redoc/{documentName}";
                config.TransformToExternalPath = transformToExternalPath;
            });

            //app.UseApiverse(config=> { });
            return app;
        }


        private static void _settings(AspNetCoreOpenApiDocumentGeneratorSettings document, NSwagConfig config, string versionPrefix)
        {
            document.Title = config?.Title ?? "WebApi 文档";
            document.Description = config?.Description
//@"天使项目 API 文档,可以使用API Key来授权测试。

//# Introduction
//This API is documented in **OpenAPI format** and is based on

//# Authentication

// Petstore offers two forms of authentication: **OpenAPI format**
//      - API Key
//      - OAuth2
//    OAuth2 - an open protocol to allow secure authorization in a simple
//    and standard method from web, mobile and desktop applications.

//"
;
            var version = config?.Version ?? "v1";
            if (string.IsNullOrWhiteSpace(config?.PathPrefix) && config?.ApiGroupNames?.Length == 1)
                config.PathPrefix = config.ApiGroupNames.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(config?.PathPrefix))
                config.PathPrefix = config.PathPrefix.Trim('/') + "/";

            document.Version = version;
            document.DocumentName = versionPrefix + config?.PathPrefix + version;
            document.ApiGroupNames = config?.ApiGroupNames;

            //document.DefaultEnumHandling = NJsonSchema.Generation.EnumHandling.CamelCaseString;
            document.GenerateExamples = true;
            document.GenerateEnumMappingDescription = true;



            document.PostProcess = (f) =>
            {
                f.Info.TermsOfService = config?.TermsOfService;
                f.Info.Contact = config?.Contact;
                f.Info.License = config?.License;
                f.ExternalDocumentation = new OpenApiExternalDocumentation { Description = "ReDoc 文档", Url = "/redoc/" + document.DocumentName };

                f.Info.ExtensionData = config?.ExtensionData;
                //f.Info.TermsOfService = "http://www.weberp.com.cn";
                //f.Info.Contact = new NSwag.OpenApiContact { Email = "mccj@weberp.com.cn", Name = "The KeWei Team", Url = "http://www.weberp.com.cn" };
                //f.Info.License = new NSwag.OpenApiLicense { Name = "Apache 2.0", Url = "http://www.apache.org/licenses/LICENSE-2.0.html" };

                //f.Info.ExtensionData = new Dictionary<string, object>();
                //f.Info.ExtensionData.Add("x-logo", new { url = "https://redocly.github.io/redoc/petstore-logo.png", altText = "Petstore logo" });

                //f.ExternalDocumentation = new OpenApiExternalDocumentation { Description = "ReDoc For {documentName}", Url = "/redoc/{documentName}" };
            };

            if (config?.OperationSecurity != null)
            {
                config?.OperationSecurity.Apply(document);
            }

            #region 基本身份认证
            //document.AddSecurity("Basic", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"Basic 授权模式",
            //    Type = NSwag.OpenApiSecuritySchemeType.Basic
            //});
            ////貌似同Basic模式
            ////document.AddSecurity("Http", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            ////{
            ////    Description = $"Http 授权模式",
            ////    Type = NSwag.OpenApiSecuritySchemeType.Http
            ////});
            //document.AddSecurity("ApiKey_Header"/*, new[] { "skoruba_identity_admin_api" }*/, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"ApiKey 授权模式,In 可选 Header 或 Query",
            //    Name = "Authorization",
            //    Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
            //    In = NSwag.OpenApiSecurityApiKeyLocation.Header
            //    //或者
            //    //In = NSwag.OpenApiSecurityApiKeyLocation.Query
            //});
            //document.AddSecurity("ApiKey_Query", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"ApiKey 授权模式,In 可选 Header 或 Query",
            //    Name = "Authorization",
            //    Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
            //    //In = NSwag.OpenApiSecurityApiKeyLocation.Header
            //    //或者
            //    In = NSwag.OpenApiSecurityApiKeyLocation.Query
            //});
            #endregion 基本身份认证
            #region OAuth2
            //var rsss = new System.Collections.Generic.Dictionary<string, string>
            //{
            //    { "skoruba_identity_admin_api", "Skoruba IdentityServer4 Admin Api" }
            //};

            //document.AddSecurity("OAuth2_Implicit", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"OAuth2_Implicit 简化模式,\r\n测试ClientId:'Test_Implicit_Development'",
            //    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
            //    Flow = NSwag.OpenApiOAuth2Flow.Implicit,
            //    Scopes = rsss,
            //    AuthorizationUrl = "http://ser_identity.service.erp.consul/connect/authorize"
            //});

            //document.AddSecurity("OAuth2_Implicit", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"OAuth2_Implicit 简化模式,\r\n测试ClientId:'Test_Implicit_Development'",
            //    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
            //    Flow = NSwag.OpenApiOAuth2Flow.Implicit,
            //    Scopes = rsss,
            //    AuthorizationUrl = "http://ser_identity.service.erp.consul/connect/authorize"
            //});
            //document.AddSecurity("OAuth2_Password", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"OAuth2_Password 密码模式,\r\n测试ClientId:'Test_Password_Development'\r\n测试ClientSecret:'test'",
            //    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
            //    Flow = NSwag.OpenApiOAuth2Flow.Password,
            //    Scopes = rsss,
            //    TokenUrl = "http://ser_identity.service.erp.consul/connect/token"
            //});
            //document.AddSecurity("OAuth2_Application", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"OAuth2_Application 客户端模式,\r\n测试ClientId:'Test_ClientCredentials_Development'\r\n测试ClientSecret:'test'",
            //    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
            //    Flow = NSwag.OpenApiOAuth2Flow.Application,
            //    Scopes = rsss,
            //    TokenUrl = "http://ser_identity.service.erp.consul/connect/token"
            //});
            //document.AddSecurity("OAuth2_AccessCode", new[] { "skoruba_identity_admin_api" }, new NSwag.OpenApiSecurityScheme
            //{
            //    Description = $"OAuth2_AccessCode 授权码模式\r\n测试ClientId:'Test_AuthorizationCode_Development'\r\n测试ClientSecret:'test'",
            //    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
            //    Flow = NSwag.OpenApiOAuth2Flow.AccessCode,
            //    Scopes = rsss,
            //    AuthorizationUrl = "http://ser_identity.service.erp.consul/connect/authorize",
            //    TokenUrl = "http://ser_identity.service.erp.consul/connect/token",
            //    //OpenIdConnectUrl = "http://ser_identity.service.erp.consul/connect/token"
            //});

            //document.AddSecurity("OAuth2_Bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
            //{
            //    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
            //    Description = $"OAuth2_Implicit 授权模式,\r\n测试ClientId:'Test_Implicit_Development'",
            //    Flow = NSwag.OpenApiOAuth2Flow.Implicit,
            //    Flows = new NSwag.OpenApiOAuthFlows()
            //    {
            //        Implicit = new NSwag.OpenApiOAuthFlow()
            //        {
            //            Scopes = rsss,
            //            AuthorizationUrl = "http://ser_identity.service.erp.consul/connect/authorize",
            //            //TokenUrl = "http://ser_identity.service.erp.consul/connect/token"
            //        },
            //    }
            //});
            #endregion OAuth2
        }
    }
    /// <summary>
    /// 解决api控制器注释显示的问题
    /// </summary>
    public class DocumentControllerTagsProcessor : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            foreach (var controllerType in context.ControllerTypes)
            {
                context.Document.Tags.Add(new NSwag.OpenApiTag
                {
                    Name = GetControllerName(controllerType),
                    Description = controllerType.GetXmlDocsSummary()
                    //ExternalDocumentation=new NSwag.OpenApiExternalDocumentation { Url= "http://swagger.io", Description= "Find out more about our store" }
                });
            }
        }

        protected virtual string GetControllerName(System.Type controllerType)
        {
            var controllerName = controllerType.Name;
            if (controllerName.EndsWith("Controller"))
            {
                controllerName = controllerName.Substring(0, controllerName.Length - 10);
            }

            return controllerName;
        }
    }

    public class ReDocCodeSampleAttribute : OpenApiOperationProcessorAttribute
    {
        public ReDocCodeSampleAttribute(string language, string source)
            : base(typeof(ReDocCodeSampleAppender), language, source)
        {
        }

        internal class ReDocCodeSampleAppender : NSwag.Generation.Processors.IOperationProcessor
        {
            private readonly string _language;
            private readonly string _source;
            private const string ExtensionKey = "x-code-samples";

            public ReDocCodeSampleAppender(string language, string source)
            {
                //var document = NSwag.SwaggerDocument.FromJsonAsync("...").Result;
                //var settings = new NSwag.CodeGeneration.CSharp.SwaggerToCSharpClientGeneratorSettings
                //{
                //    ClassName = "MyClass",
                //    CSharpGeneratorSettings =
                //    {
                //        Namespace = "MyNamespace"
                //    }
                //};
                //var generator = new NSwag.CodeGeneration.CSharp.SwaggerToCSharpClientGenerator(document, settings);



                _language = language;
                _source = source;
            }

            public bool Process(OperationProcessorContext context)
            {
                if (context.OperationDescription.Operation.ExtensionData == null)
                    context.OperationDescription.Operation.ExtensionData = new Dictionary<string, object>();

                var data = context.OperationDescription.Operation.ExtensionData;
                if (!data.ContainsKey(ExtensionKey))
                    data[ExtensionKey] = new List<ReDocCodeSample>();

                var samples = (List<ReDocCodeSample>)data[ExtensionKey];
                samples.Add(new ReDocCodeSample
                {
                    Language = _language,
                    Source = _source,
                });

                return true;
            }
        }

        internal class ReDocCodeSample
        {
            [JsonProperty("lang")]
            public string Language { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }
        }
    }
    public abstract class IOperationSecurity
    {
        public string SecurityName { get; set; }
        public virtual string Description { get; set; }
        public abstract void Apply(AspNetCoreOpenApiDocumentGeneratorSettings document);
    }
    public class ApiKeySecurityScheme : IOperationSecurity
    {
        public string Name { get; set; } = "Authorization";
        public ApiKeyLocation Location { get; set; }
        public enum ApiKeyLocation
        {
            Query = 1,
            Header = 2
        }
        public override void Apply(AspNetCoreOpenApiDocumentGeneratorSettings document)
        {
            var _in = System.Enum.Parse<OpenApiSecurityApiKeyLocation>(this?.Location.ToString());
            document.AddSecurity(this?.SecurityName ?? SwaggerNSwagExtensions.DefaultSecurityName/*, new[] { "skoruba_identity_admin_api" }*/, new NSwag.OpenApiSecurityScheme
            {
                Description = this?.Description,
                Name = this?.Name,
                Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                In = _in
            });
        }
    }
    public class BasicSecurityScheme : IOperationSecurity
    {
        public string Name { get; set; } = "Authorization";
        public ApiKeyLocation Location { get; set; }
        public enum ApiKeyLocation
        {
            Query = 1,
            Header = 2
        }
        public override void Apply(AspNetCoreOpenApiDocumentGeneratorSettings document)
        {
            var _in = System.Enum.Parse<OpenApiSecurityApiKeyLocation>(this?.Location.ToString());
            document.AddSecurity(this?.SecurityName ?? SwaggerNSwagExtensions.DefaultSecurityName/*, new[] { "skoruba_identity_admin_api" }*/, new NSwag.OpenApiSecurityScheme
            {
                Description = this?.Description,
                Name = this?.Name,
                Type = NSwag.OpenApiSecuritySchemeType.Basic
            });
        }
    }

    public class NSwagConfig : NJsonSchema.JsonExtensionObject
    {
        public string PathPrefix { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string[] ApiGroupNames { get; set; }
        public string Description { get; set; }
        public string TermsOfService { get; set; }
        public OpenApiContact Contact { get; set; }
        public OpenApiLicense License { get; set; }
        public IOperationSecurity OperationSecurity { get; set; }
    }
}
