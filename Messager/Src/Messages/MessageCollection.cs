using ClientServerLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class MessageCollection: ObservableCollection<Message>
    {
        public void AddMessage(MessageInfo messageInfo)
        {
            Add(new Message(messageInfo));
        }
    }
}
