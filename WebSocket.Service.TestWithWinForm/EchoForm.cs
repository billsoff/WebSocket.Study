using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WebSocketService.Test;

namespace WebSocketService.TestWithWinForm
{
    public partial class EchoForm : Form, INotifier
    {
        private WebSocketServer<EchoJob> _server;

        public EchoForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _server = new WebSocketServer<EchoJob>(
                    "http://127.0.0.1:8089/",
                    new EchoJobInitializer("^_^", this)
                );
            Task _ = _server.StartAsync();
        }

        public void Notify(string message)
        {
            notifyIcon1.ShowBalloonTip(
                    5000,
                    "New Message",
                    message,
                    ToolTipIcon.Info
                );
        }
    }
}
