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
    [Serializable]
    public class MessageCollection: ObservableCollection<Message>
    {
        public void AddMessage(MessageInfo messageInfo)
        {
            if (!this.ToList().Exists(el => el.MessageId == messageInfo.MessageId))
                Add(new Message(messageInfo));
        }
    }
}
