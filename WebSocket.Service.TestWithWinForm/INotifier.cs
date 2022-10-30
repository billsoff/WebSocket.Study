using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketService.TestWithWinForm
{
    internal interface INotifier
    {
        void Notify(string message);
    }
}
