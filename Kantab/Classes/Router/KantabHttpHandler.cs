using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kantab.Classes.Router.Middleware;

namespace Kantab.Classes.Router;

public class KantabHttpHandler
{
    private Dictionary<Tuple<Regex, string>, Func<KantabHttpContext, Task>[]> _routeMappings = new();


    private void AddRoute(Regex routePattern, string method, params Func<KantabHttpContext, Task>[] middleware)
    {
        _routeMappings.Add(new(routePattern, method), middleware);
    }

    public void Get(Regex routePattern, params Func<KantabHttpContext, Task>[] middleware) {
        _routeMappings.Add(new(routePattern, "GET"), middleware);
    }

    public void Post(Regex routePattern, params Func<KantabHttpContext, Task>[] middleware)
    {
        _routeMappings.Add(new(routePattern, "POST"), middleware);
    }

    public void Patch(Regex routePattern, params Func<KantabHttpContext, Task>[] middleware)
    {
        _routeMappings.Add(new(routePattern, "PATCH"), middleware);
    }

    public void Put(Regex routePattern, params Func<KantabHttpContext, Task>[] middleware)
    {
        _routeMappings.Add(new(routePattern, "PUT"), middleware);
    }

    public async Task RunHandler(HttpListenerContext ctx)
    {
        KantabHttpContext kCtx = new(ctx);
        try
        {
            KeyValuePair<Tuple<Regex, string>, Func<KantabHttpContext, Task>[]>? matchedRoute;
            matchedRoute =
                _routeMappings.FirstOrDefault(mapping =>
                        // the first regex that matches:
                        // the URL, minus the leading slash (/)
                        // that [1..] is C# 8's Range expression
                        // and also: has the same HTTP method
                        mapping.Key.Item1.IsMatch(ctx.Request.RawUrl[1..]) && mapping.Key.Item2 == ctx.Request.HttpMethod);

            if (matchedRoute.Value.Key == null)
            {
                // route not found
                await RunError(kCtx, HttpStatusCode.NotFound);
            }
            else
            {
                // try each middleware in order
                foreach (var middleware in matchedRoute.Value.Value)
                {
                    if (kCtx.ResponseSent) break; // stop if response was sent
                    await middleware.Invoke(kCtx);
                }
            }

            // if by the end of the middleware chain no response was sent, send the latest status (or 404 in its absence)
            if (!kCtx.ResponseSent)
            {
                await RunError(kCtx, kCtx.Status);
            }
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            await RunError(kCtx, HttpStatusCode.InternalServerError);
        }
    }

    public async Task RunError(KantabHttpContext ctx, HttpStatusCode? status = null)
    {
        await KantabHttpError.Run(ctx, status ?? HttpStatusCode.NotFound);
    }
}