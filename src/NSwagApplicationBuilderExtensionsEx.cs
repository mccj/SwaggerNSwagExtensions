//最后修改 2020-03-13

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NSwag.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SwaggerExtensions
{
    public static class NSwagApplicationBuilderExtensionsEx
    {
        public static IApplicationBuilder UseReDocEx(this IApplicationBuilder app, Action<ReDocSettings>? configure = default)
        {
            var settings = app.ApplicationServices.GetService<IOptions<ReDocSettings>>()?.Value ?? new ReDocSettings();
            configure?.Invoke(settings);
            UseSwaggerUiWithDocumentNamePlaceholderExpanding(app, settings, delegate (string swaggerRoute, string swaggerUiRoute)
            {
                app.UseMiddleware<RedirectToIndexMiddleware>(new object[3]
                {
                    swaggerUiRoute,
                    swaggerRoute,
                    settings.TransformToExternalPath
                });
                var swaggerUiIndexMiddlewareType = Type.GetType("NSwag.AspNetCore.Middlewares.SwaggerUiIndexMiddleware,NSwag.AspNetCore");
                //app.UseMiddleware<SwaggerUiIndexMiddleware>(
                app.UseMiddleware(swaggerUiIndexMiddlewareType!,
                    new object[3]
                {
                    swaggerUiRoute + "_index.html",
                    settings,
                    "NSwag.AspNetCore.ReDoc.index.html"
                });
                app.UseFileServer(new FileServerOptions
                {
                    RequestPath = new PathString(swaggerUiRoute),
                    FileProvider = new EmbeddedFileProvider(typeof(NSwagApplicationBuilderExtensions).GetTypeInfo().Assembly, "NSwag.AspNetCore.ReDoc")
                });
            }, (IEnumerable<OpenApiDocumentRegistration> documents) => false);
            return app;
        }
        private static void UseSwaggerUiWithDocumentNamePlaceholderExpanding(IApplicationBuilder app, SwaggerUiSettingsBase settings, Action<string, string> register, Func<IEnumerable<OpenApiDocumentRegistration>, bool> registerMultiple)
        {
            var actualSwaggerDocumentPath = settings.DocumentPath.Substring(settings.MiddlewareBasePath?.Length ?? 0);
            var actualSwaggerUiPath = settings.Path.Substring(settings.MiddlewareBasePath?.Length ?? 0);
            if (actualSwaggerDocumentPath.Contains("{documentName}"))
            {
                IEnumerable<OpenApiDocumentRegistration> requiredService = app.ApplicationServices.GetRequiredService<IEnumerable<OpenApiDocumentRegistration>>();
                if (actualSwaggerUiPath.Contains("{documentName}"))
                {
                    foreach (OpenApiDocumentRegistration item in requiredService)
                    {
                        register(actualSwaggerDocumentPath.Replace("{documentName}", Common.ToBase64(item.DocumentName)), actualSwaggerUiPath.Replace("{documentName}", Common.ToBase64(item.DocumentName)));
                    }
                }
                else if (registerMultiple(requiredService))
                {
                    register(actualSwaggerDocumentPath, actualSwaggerUiPath);
                }
                else
                {
                    if (requiredService.Count() != 1)
                    {
                        throw new NotSupportedException("This UI does not support multiple documents per UI: Do not use '{documentName}' placeholder in DocumentPath or Path.");
                    }
                    register(actualSwaggerDocumentPath.Replace("{documentName}", Common.ToBase64(requiredService.First().DocumentName)), actualSwaggerUiPath.Replace("{documentName}", Common.ToBase64(requiredService.First().DocumentName)));
                }
            }
            else
            {
                if (actualSwaggerUiPath.Contains("{documentName}"))
                {
                    throw new ArgumentException("The SwaggerUiRoute cannot contain '{documentName}' placeholder when SwaggerRoute is missing the placeholder.");
                }
                register(actualSwaggerDocumentPath, actualSwaggerUiPath);
            }
        }
    }
    internal class RedirectToIndexMiddleware
    {
        private readonly RequestDelegate _nextDelegate;

        private readonly string _swaggerUiRoute;

        private readonly string _swaggerRoute;

        private readonly Func<string, HttpRequest, string> _transformToExternal;

        public RedirectToIndexMiddleware(RequestDelegate nextDelegate, string internalSwaggerUiRoute, string internalSwaggerRoute, Func<string, HttpRequest, string> transformToExternal)
        {
            _nextDelegate = nextDelegate;
            _swaggerUiRoute = internalSwaggerUiRoute;
            _swaggerRoute = internalSwaggerRoute;
            _transformToExternal = transformToExternal;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.HasValue || !string.Equals(context.Request.Path.Value.Trim(new char[1] { '/' }), _swaggerUiRoute.Trim(new char[1] { '/' }), StringComparison.OrdinalIgnoreCase))
            {
                await _nextDelegate(context);
                return;
            }
            var base64 = _swaggerUiRoute.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            var path = Common.FromBase64(base64);
            var swaggerRoute = _swaggerRoute.Replace(base64, path);
            context.Response.StatusCode = 302;
            string text = ((!string.IsNullOrWhiteSpace(swaggerRoute)) ? ("?url=" + _transformToExternal(swaggerRoute, context.Request)) : "");
            string text2 = _transformToExternal(_swaggerUiRoute, context.Request);
            context.Response.Headers.Add("Location", ((text2 != "/") ? text2 : "") + "_index.html" + text);
        }
    }
}
