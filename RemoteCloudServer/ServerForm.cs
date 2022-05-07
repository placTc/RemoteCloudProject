using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace RemoteCloudServer
{
    public partial class ServerForm : Form
    {
        private static AsynchronousServer server = new AsynchronousServer();
        public ServerForm()
        {
            Thread serverThread = new Thread(new ThreadStart(AsynchronousServer.StartThreads));
            serverThread.IsBackground = true;
            serverThread.Start();

            InitializeComponent();
        }
    }
}
