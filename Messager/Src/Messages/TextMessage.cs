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
        string messageText;

        public string MessageText
        {
            get => messageText;
            set
            {
                if (messageText != value)
                {
                    messageText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MessageText)));
                }
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
