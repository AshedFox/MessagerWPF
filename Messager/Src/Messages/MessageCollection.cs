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
        public void AddTextMessage(MessageInfo messageInfo)
        {
            Add(new TextMessage { MessageInfo = messageInfo });
        }

        public void AddAudioMessage(string path)
        {
            Add(new AudioMessage { Path = path });
        }        
        
        public void AddVideoMessage(string path)
        {
            Add(new VideoMessage { Path = path });
        }        
        
        public void AddImageMessage(string path)
        {
            Add(new ImageMessage { Path = path });
        }        
        
        public void AddFileMessage(string path)
        {
            Add(new FileMessage { Path = path });
        }
    }
}
