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

        public void AddAudioMessage(AttachmentInfo messageInfo)
        {
            Add(new AudioMessage { MessageInfo = messageInfo });
        }        
        
        public void AddVideoMessage(AttachmentInfo messageInfo)
        {
            Add(new VideoMessage { MessageInfo = messageInfo });
        }        
        
        public void AddImageMessage(AttachmentInfo messageInfo)
        {
            Add(new ImageMessage { MessageInfo = messageInfo });
        }        
        
        public void AddFileMessage(AttachmentInfo messageInfo)
        {
            Add(new FileMessage { MessageInfo = messageInfo });
        }
    }
}
