using ClientServerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    class ImageMessage : Message
    {
        AttachmentInfo messageInfo;
        private string message;

        public AttachmentInfo MessageInfo
        {
            get => messageInfo;
            set
            {
                if (messageInfo != value)
                {
                    messageInfo = value;
                    Message = System.IO.Path.Combine(MainWindow.attachmentsPath,
                                                     System.IO.Path.ChangeExtension(messageInfo.Filename,
                                                                                    messageInfo.Extension));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(messageInfo)));
                }
            }
        }
        public string Message
        {
            get => message;
            set
            {
                message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(message)));
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
