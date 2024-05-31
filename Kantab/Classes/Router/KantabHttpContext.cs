using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kantab.Classes.Router; 

public class KantabHttpContext {
    public bool ResponseSent { get; private set; }

    public HttpListenerContext AdvancedContext { get; }

    public HttpStatusCode? Status { get; set; }

    public KantabHttpContext(HttpListenerContext ctx) {
        AdvancedContext = ctx;
    }

    public async Task Respond(string body, string contentType = "application/octet-stream",
        HttpStatusCode code = HttpStatusCode.OK) {
        byte[] textBody = Encoding.UTF8.GetBytes(body);
        await Respond(textBody, contentType, code);
    }

    public async Task Respond(byte[] body, string contentType = "application/octet-stream",
        HttpStatusCode code = HttpStatusCode.OK) {
        AdvancedContext.Response.ContentLength64 = body.Length;
        AdvancedContext.Response.ContentType = contentType;
        AdvancedContext.Response.StatusCode = (int) code;
        await AdvancedContext.Response.OutputStream.WriteAsync(body);
        AdvancedContext.Response.OutputStream.Close();
        AdvancedContext.Response.Close();
        ResponseSent = true;
    }
}