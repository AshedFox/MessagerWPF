using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    class AudioMessage : Message
    {
        //byte[] audioData;
        string audioPath;

        /*        public byte[] AudioData
                {
                    get => audioData;
                    set
                    {
                        if (audioData != value)
                        {
                            audioData = value;
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(audioData)));
                        }
                    }
                }
        */

        public string AudioPath
        {
            get => audioPath;
            set
            {
                if (audioPath != value)
                {
                    audioPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(audioPath)));
                }
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler PlayStatusChanged;
    }

}
