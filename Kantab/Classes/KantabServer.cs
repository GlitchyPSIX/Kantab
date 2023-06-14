using System.Collections.Generic;
using System.Net;
using Kantab.WinAPI.Structs;

namespace Kantab.Classes; 

public class KantabServer {

    public short Port = 8337;
    private string _prefix => $"http://localhost:{Port}";

    private HttpListener _listener;
    private List<KantabClient> _clients;

    public KantabServer(KantabServerConfig config) {
        Port = config.Port;
    }

    // NEXT: Do basic webserver and websocket upgrade


}