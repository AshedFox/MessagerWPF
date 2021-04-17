using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerLib
{
    public class MessageInfo
    {
        public long messageId;
        public string senderName;
        public string sendDateTime;
        public string messageText;

        public MessageInfo()
        {
        }

        public MessageInfo(long messageId, string senderName, string sendDateTime, string messageText)
        {
            this.messageId = messageId;
            this.senderName = senderName;
            this.sendDateTime = sendDateTime;
            this.messageText = messageText;
        }

        public override string ToString()
        {
            return $"{messageId}\n{senderName}\n{sendDateTime}\n{messageText}";
        }
    }
}
