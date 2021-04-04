using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    class ClientManager
    {
        #region Singleton

        private static readonly Lazy<ClientManager> instance =
            new Lazy<ClientManager>(() => new ClientManager());

        #endregion

        public ClientManager()
        {
            Client = new Client.Client("127.0.0.1", 40000);
        }

        public static ClientManager Instance => instance.Value;

        public Client.Client Client { get => client; set => client = value; }

        Client.Client client;
    }
}
