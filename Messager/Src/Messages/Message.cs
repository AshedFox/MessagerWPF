using ClientServerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    [Serializable]
    public class Message : INotifyPropertyChanged
    {
        private long messageId;
        private long chatId;
        private long senderId;
        private string sendDateTime;
        private string messageText;
        AttachmentsCollection attachments = new AttachmentsCollection();

        public Message()
        {
        }

        public Message(long messageId,
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
            SenderId = messageInfo.SenderId;
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
        public string SendDateTime 
        {
            get
            {
                return DateTime.Parse(sendDateTime).ToLocalTime().ToString(@"yyyy-MM-dd HH:mm:ss");
            }
            set
            {
                if (sendDateTime != value)
                {
                    sendDateTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(sendDateTime)));
                }
            }
        }
        public long SenderId { 
            get => senderId;
            set
            {
                if (senderId != value)
                {
                    senderId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(senderId)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SenderName)));
                }
            }
        }

        public string SenderName
        {
            get
            {
                if (PagesManager.Instance.ConversationPage.ChatUsers.TryGetValue(senderId, out string name))
                {
                    return name;
                }
                else return "???";
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

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
