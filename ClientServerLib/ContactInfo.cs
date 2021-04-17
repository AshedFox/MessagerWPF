using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerLib
{
    public class ContactInfo
    {
        public long id;
        public string name;

        public ContactInfo()
        {
        }

        public ContactInfo(long id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override string ToString()
        {
            return $"{id}\n{name}";
        }
    }
}
