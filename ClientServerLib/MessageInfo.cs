using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerLib
{
    public class MessageInfo
    {
        private long messageId;
        private long chatId;
        private string senderName;
        private string sendDateTime;
        private string messageText;
        List<AttachmentInfo> attachmentsInfo = new List<AttachmentInfo>();

        public MessageInfo()
        {
        }

        public MessageInfo(long messageId,
                           long chatId,
                           string senderName,
                           string sendDateTime,
                           string messageText,
                           List<AttachmentInfo> attachmentsInfo)
        {
            MessageId = messageId;
            ChatId = chatId;
            SenderName = senderName;
            SendDateTime = sendDateTime;
            MessageText = messageText;
            AttachmentsInfo = attachmentsInfo;
        }

        public List<AttachmentInfo> AttachmentsInfo { get => attachmentsInfo; set => attachmentsInfo = value; }
        public string MessageText { get => messageText; set => messageText = value; }
        public string SendDateTime { get => sendDateTime; set => sendDateTime = value; }
        public string SenderName { get => senderName; set => senderName = value; }
        public long MessageId { get => messageId; set => messageId = value; }
        public long ChatId { get => chatId; set => chatId = value; }
    }
}
