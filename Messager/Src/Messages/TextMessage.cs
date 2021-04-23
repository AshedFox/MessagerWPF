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
                    Message = messageInfo.SendDateTime + ' ' + messageInfo.SenderName +
                              ":\n" + messageInfo.MessageText;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MessageInfo)));
                }
            }
        }

        public string Message { get => message; 
            set
            {
                message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
