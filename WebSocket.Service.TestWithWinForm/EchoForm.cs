using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSocketService.TestWithWinForm
{
    public partial class EchoForm : Form, INotifier
    {
        private WebSocketServer _server;

        public EchoForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _server = new WebSocketServer(
                    "http://127.0.0.1:8089/",
                    new JobFactory("^_^", this)
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
