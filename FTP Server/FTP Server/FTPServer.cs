using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FTP_Server
{
    class FTPServer
    {
        private TcpListener _listener;

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 21);
            _listener.Start();
            _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);
        }

        public void Stop()
        {
            if (_listener != null)
            {
                _listener.Stop();
            }
        }

        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            TcpClient client = _listener.EndAcceptTcpClient(result);
            _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);

            NetworkStream stream = client.GetStream();

            using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
            using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
            {
                writer.WriteLine("YOU CONNECTED TO ME");
                writer.Flush();
                writer.WriteLine("I will repeat after you. Send a blank line to quit.");
                writer.Flush();

                string line = null;

                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    writer.WriteLine("Echoing back: {0}", line);
                    writer.Flush();
                }
            }
        }
    }
}
