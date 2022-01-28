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

namespace SwaggerExtensions
{
    public static class Common
    {
        public static string ToBase64(string str)
        {
            char[] padding = { '=' };
            string returnValue = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str)).TrimEnd(padding).Replace('+', '-').Replace('/', '_');
            return returnValue;
        }

        public static string FromBase64(string str)
        {
            string incoming = str.Replace('_', '/').Replace('-', '+');
            switch (str.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(incoming);
            string originalText = System.Text.Encoding.UTF8.GetString(bytes);
            return originalText;
        }
    }
}
