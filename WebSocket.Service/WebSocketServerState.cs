using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketService
{
    public enum WebSocketServerState
    {
        Initial,

        Running,

        Fault,

        Stopping,

        Stopped,
    }
}
