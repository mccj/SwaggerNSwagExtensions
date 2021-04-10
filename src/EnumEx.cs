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
    public static class EnumEx
    {
        public static TEnum Parse<TEnum>(string value) where TEnum : struct
        {
           return (TEnum)System.Enum.Parse(typeof(TEnum), value);
        }
    }
}
