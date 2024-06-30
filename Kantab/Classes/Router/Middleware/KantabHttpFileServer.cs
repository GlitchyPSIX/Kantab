using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kantab.Classes.Router.Middleware;

public static class KantabHttpFileServer
{
    /// <summary>
    /// Serves files according to directory structure.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="basePath"></param>
    /// <param name="skipSegments"></param>
    /// <returns></returns>
    public static async Task ServeDirectory(KantabHttpContext ctx, string basePath, int skipSegments = 0)
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

    /// <summary>
    /// Serves a singular unchanged file.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task ServeSingle(KantabHttpContext ctx, string path)
    {
        if (ctx.AdvancedContext.Request.Url == null) return;

        string physicalPath = path.Replace('\\', '/');

        if (!File.Exists(physicalPath))
        {
            ctx.Status = HttpStatusCode.NotFound;
            return;
        }

        byte[] fileBytes = await File.ReadAllBytesAsync(physicalPath);
        string mimeType = MimeMapping.MimeUtility.GetMimeMapping(Path.GetExtension(physicalPath).Replace(".", ""));
        await ctx.Respond(fileBytes, mimeType);
    }

    /// <summary>
    /// Serves a string as a specified MIME type.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="path"></param>
    /// <param name="mimeType"></param>
    /// <returns></returns>
    public static async Task ServeText(KantabHttpContext ctx, string content, string mimeType)
    {
        if (ctx.AdvancedContext.Request.Url == null) return;

        byte[] fileBytes = Encoding.UTF8.GetBytes(content);
        await ctx.Respond(fileBytes, mimeType);
    }
}