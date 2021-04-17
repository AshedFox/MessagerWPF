using ClientServerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class TextMessage:Message
    {
        string message;

        MessageInfo messageInfo;

        public MessageInfo MessageInfo
        {
            get => messageInfo;
            set
            {
                if (messageInfo != value)
                {
                    messageInfo = value;
                    Message = messageInfo.sendDateTime + ' ' + messageInfo.senderName +
                              ":\n" + messageInfo.messageText;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MessageInfo)));
                }
            }
        }

        public string Message { get => message; set => message = value; }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
