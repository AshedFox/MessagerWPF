using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerLib
{
    public class MessageInfo
    {
        private long messageId;
        private long chatId;
        private long senderId;
        private string sendDateTime;
        private string messageText;
        List<AttachmentInfo> attachmentsInfo = new List<AttachmentInfo>();

        public MessageInfo()
        {
        }

        public MessageInfo(long messageId,
                           long chatId,
                           long senderId,
                           string sendDateTime,
                           string messageText,
                           List<AttachmentInfo> attachmentsInfo)
        {
            MessageId = messageId;
            ChatId = chatId;
            SenderId = senderId;
            SendDateTime = sendDateTime;
            MessageText = messageText;
            AttachmentsInfo = attachmentsInfo;
        }

        public List<AttachmentInfo> AttachmentsInfo { get => attachmentsInfo; set => attachmentsInfo = value; }
        public string MessageText { get => messageText; set => messageText = value; }
        public string SendDateTime { get => sendDateTime; set => sendDateTime = value; }
        public long SenderId { get => senderId; set => senderId = value; }
        public long MessageId { get => messageId; set => messageId = value; }
        public long ChatId { get => chatId; set => chatId = value; }
    }
}
