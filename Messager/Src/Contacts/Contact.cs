using ClientServerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class Contact:INotifyPropertyChanged
    {
        long id;
        string name;
        string lastMessage;

        public long Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(id)));
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

        public string LastMessage
        {
            get => lastMessage;
            set
            {
                if (lastMessage != value)
                {
                    lastMessage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(lastMessage)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
