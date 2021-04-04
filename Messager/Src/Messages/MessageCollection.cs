using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class MessageCollection: ObservableCollection<Message>
    {
        public void AddTextMessage(string message)
        {
            Add(new TextMessage { MessageText = message });
        }

        public void AddAudioMessage(string audioPath)
        {
            Add(new AudioMessage { AudioPath = audioPath });
        }
    }
}
