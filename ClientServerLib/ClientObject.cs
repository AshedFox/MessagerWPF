using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ClientServerLib
{
    public class ClientObject
    {
        long id;
        string username;

        TcpClient client;

        bool isAutorized;

        public ClientObject(TcpClient client)
        {
            Client = client;
        }

        public TcpClient Client { get => client; private set => client = value; }
        public string Username { get => username; set => username = value; }
        public long Id { get => id; set => id = value; }
        public bool IsAutorized { get => isAutorized; set => isAutorized = value; }
    }
}
