# SwaggerNSwagExtensions

[![Build status](https://ci.appveyor.com/api/projects/status/r1mkt9s81imqv3gp?svg=true)](https://ci.appveyor.com/project/mccj/swaggernswagextensions)
[![MyGet](https://img.shields.io/myget/mccj/vpre/SwaggerNSwagExtensions.svg)](https://myget.org/feed/mccj/package/nuget/SwaggerNSwagExtensions)
[![NuGet](https://buildstats.info/nuget/SwaggerNSwagExtensions?includePreReleases=false)](https://www.nuget.org/packages/SwaggerNSwagExtensions)
[![MIT License](https://img.shields.io/badge/license-MIT-orange.svg)](https://github.com/mccj/SwaggerNSwagExtensions/blob/master/LICENSE)


## Features

 - **简化 NSwag 的使用

## Setup

ASP.NET Core Applications
```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddNSwagSwagger(new NSwagConfig { ApiGroupNames = new[] { "test_V1" } });
        services.AddNSwagSwagger(new NSwagConfig { ApiGroupNames = new[] { "test_V2" } });
        services.AddNSwagSwagger(new NSwagConfig { });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseNSwagSwaggerUI();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```
