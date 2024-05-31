using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Kantab.Classes.Router.Middleware;

public static class KantabHttpFileServer
{
    public static async Task Run(KantabHttpContext ctx, string basePath, int skipSegments = 0)
    {
        if (ctx.AdvancedContext.Request.Url == null) return;
        IEnumerable<string> initialPath = ctx.AdvancedContext.Request.Url.Segments;
        string[] realPathSegments = initialPath.Skip(skipSegments + 1)
            .Prepend(basePath)
            .ToArray();
        string physicalPath = Path.Join(realPathSegments).Replace('\\', '/');

        if (!File.Exists(physicalPath))
        {
            ctx.Status = HttpStatusCode.NotFound;
            return;
        }

        byte[] fileBytes = await File.ReadAllBytesAsync(physicalPath);
        string mimeType = MimeMapping.MimeUtility.GetMimeMapping(Path.GetExtension(physicalPath).Replace(".", ""));
        await ctx.Respond(fileBytes, mimeType);

    }
}