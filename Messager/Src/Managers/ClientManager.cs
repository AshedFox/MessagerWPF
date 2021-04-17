using ClientServerLib;
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

        public Client.Client Client { get => client; private set => client = value; }
        public List<ContactInfo> AvalibleContacts { get => avalibleContacts; set => avalibleContacts = value; }
        public ClientInfo ClientInfo { get => clientInfo; set => clientInfo = value; }

        Client.Client client;

        ClientInfo clientInfo;

        List<ContactInfo> avalibleContacts = new List<ContactInfo>();

        public void SetClientInfo(long id, string login, string email, string password, string name)
        {
            ClientInfo = new ClientInfo(id, login, email, "", name);
        }
    }
}
