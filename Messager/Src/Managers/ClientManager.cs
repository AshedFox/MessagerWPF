using ClientServerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            Client = new Client.Client("192.168.100.7", 40000);
        }

        public static ClientManager Instance => instance.Value;

        public Client.Client Client { get => client; private set => client = value; }
        public ClientInfo ClientInfo { get => clientInfo; set => clientInfo = value; }

        Client.Client client;

        ClientInfo clientInfo;

        public void SetClientInfo(long id, string login, string email, byte[] password, string name)
        {
            ClientInfo = new ClientInfo(id, login, email, password, name);
            PagesManager.Instance.MainMenuPage?.UserAutorized(id);
        }
    }
}
