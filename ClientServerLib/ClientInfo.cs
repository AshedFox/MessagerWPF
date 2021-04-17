using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerLib
{
    public class ClientInfo
    {
        public long id;
        public string login;
        public string email;
        public string password;
        public string name;

        public ClientInfo()
        {
        }

        public ClientInfo(string login, string email, string password, string name)
        {
            this.login = login;
            this.email = email;
            this.password = password;
            this.name = name;
        }

        public ClientInfo(long id, string login, string email, string password, string name)
        {
            this.id = id;
            this.login = login;
            this.email = email;
            this.password = password;
            this.name = name;
        }

        public override string ToString()
        {
            return $"{id}\n{login}\n{email}\n{password}\n{name}";
        }
    }
}
