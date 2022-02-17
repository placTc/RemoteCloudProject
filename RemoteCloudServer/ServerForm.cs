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
        private static AsynchronousSocketListener server = new AsynchronousSocketListener();
        public ServerForm()
        {
            Thread serverThread = new Thread(new ThreadStart(AsynchronousSocketListener.StartThreads));
            serverThread.IsBackground = true;
            serverThread.Start();

            InitializeComponent();
        }
    }
}
