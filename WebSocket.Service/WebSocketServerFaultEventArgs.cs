using System;
using System.Net;

namespace WebSocketService
{
    public sealed class WebSocketServerFaultEventArgs : EventArgs
    {
        public WebSocketServerFaultEventArgs(HttpListenerException exception)
        {
            Exception = exception;
        }

        public HttpListenerException Exception { get; private set; }
    }
}
