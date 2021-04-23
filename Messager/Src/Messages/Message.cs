using ClientServerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class Message
    {
        private long messageId;
        private long chatId;
        private string senderName;
        private string sendDateTime;
        private string messageText;
        AttachmentsCollection attachments = new AttachmentsCollection();

        public Message()
        {
        }

        public Message(long messageId,
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
            foreach (var item in attachmentsInfo)
            {
                switch (item.Type)
                {
                    case DataPrefix.Audio:
                        Attachments.Add(new AudioAttachment(item.Filename, item.Name, item.Extension));
                        break;
                    case DataPrefix.Video:
                        Attachments.Add(new VideoAttachment(item.Filename, item.Name, item.Extension));
                        break;
                    case DataPrefix.Image:
                        Attachments.Add(new ImageAttachment(item.Filename, item.Name, item.Extension));
                        break;
                    case DataPrefix.File:
                        Attachments.Add(new FileAttachment(item.Filename, item.Name, item.Extension));
                        break;
                    default:
                        break;
                }
            }

        }

        public Message(MessageInfo messageInfo)
        {
            MessageId = messageInfo.MessageId;
            ChatId = messageInfo.ChatId;
            SenderName = messageInfo.SenderName;
            SendDateTime = messageInfo.SendDateTime;
            MessageText = messageInfo.MessageText;
            foreach (var item in messageInfo.AttachmentsInfo)
            {
                Attachments.AddAttachment(item);
            }
        }

        public AttachmentsCollection Attachments { get => attachments; set => attachments = value; }
        public string MessageText { get => messageText; 
            set 
            {
                if (messageText != value)
                {
                    messageText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(messageText)));
                }
            }
        }
        public string SendDateTime { get => sendDateTime;
            set
            {
                if (sendDateTime != value)
                {
                    sendDateTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(sendDateTime)));
                }
            }
        }
        public string SenderName { get => senderName;
            set
            {
                if (senderName != value)
                {
                    senderName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(senderName)));
                }
            }
        }
        public long MessageId { get => messageId;
            set
            {
                if (messageId != value)
                {
                    messageId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(messageId)));
                }
            }
        }
        public long ChatId { get => chatId; 
            set
            {
                if (chatId != value)
                {
                    chatId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(chatId)));
                }
            }
        }

        public string MessageHeader { get => $"{SendDateTime}  {SenderName}:"; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
