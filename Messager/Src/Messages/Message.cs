using ClientServerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public abstract class Message
    {
        public abstract event PropertyChangedEventHandler PropertyChanged;
    }
}
