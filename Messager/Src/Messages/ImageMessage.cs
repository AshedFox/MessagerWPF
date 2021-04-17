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
        string path;

        public string Path
        {
            get => path;
            set
            {
                if (path != value)
                {
                    path = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(path)));
                }
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
