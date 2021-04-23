using ClientServerLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class AttachmentsCollection : ObservableCollection<Attachment>
    {
        public void AddAttachment(AttachmentInfo attachmentInfo)
        {
            switch (attachmentInfo.Type)
            {
                case DataPrefix.Audio:
                    Add(new AudioAttachment(attachmentInfo.Filename,
                                            attachmentInfo.Name,
                                            attachmentInfo.Extension));
                    break;
                case DataPrefix.Video:
                    Add(new VideoAttachment(attachmentInfo.Filename,
                                            attachmentInfo.Name,
                                            attachmentInfo.Extension));
                    break;
                case DataPrefix.Image:
                    Add(new ImageAttachment(attachmentInfo.Filename,
                                            attachmentInfo.Name,
                                            attachmentInfo.Extension));
                    break;
                case DataPrefix.File:
                    Add(new FileAttachment(attachmentInfo.Filename,
                                           attachmentInfo.Name,
                                           attachmentInfo.Extension));
                    break;
                default:
                    break;
            }
        }
    }
}
