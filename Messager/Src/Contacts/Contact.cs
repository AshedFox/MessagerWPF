using ClientServerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class Contact
    {
        long id;
        string name;

        public long Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                }
            }
        }        
        
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(name)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
