using System;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Kantab.Properties;
using Microsoft.AspNetCore.WebUtilities;

namespace Kantab.Classes.Router.Middleware;

public static class KantabHttpError
{
    private static HandlebarsTemplate<object, object> precompiledTemplate = Handlebars.Compile(Resources.ServerError);
    public static async Task Run(KantabHttpContext ctx, HttpStatusCode? status)
    {
        status ??= HttpStatusCode.NotFound;
        var handlebarsData = new
        {
            scode = (int)status,
            smsg = ReasonPhrases.GetReasonPhrase((int)status),
            version = Assembly.GetExecutingAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
        };
        string httpString = precompiledTemplate(handlebarsData);
        
        byte[] byteResponse = Encoding.UTF8.GetBytes(httpString);
        await ctx.Respond(httpString, "text/html", (HttpStatusCode)status);
    }
}