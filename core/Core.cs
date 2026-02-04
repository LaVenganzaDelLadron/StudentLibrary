using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace StudentLibrary.core
{
    internal class Core
    {
        internal TcpClient client;
        internal NetworkStream stream;

        public Core()
        {
        }

        public async Task ConnectAsync()
        {
            try
            {
                // Close existing connection if any
                if (client != null && client.Connected)
                {
                    return; // Already connected
                }

                client = new TcpClient("127.0.0.1", 4000);
                stream = client.GetStream();
                Console.WriteLine("Connected to server");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                    client = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disconnect error: {ex.Message}");
            }
        }
    }
}
