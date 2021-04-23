using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerLib
{
    public class AttachmentInfo
    {
        private string filename;
        private string name;
        private string extension;
        DataPrefix type;

        public AttachmentInfo()
        {
        }

        public AttachmentInfo(string filename, string name, string extension, DataPrefix type)
        {
            Filename = filename;
            Name = name;
            Extension = extension;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Filename}\n{Name}\n{Extension}\n";
        }


        public string Filename { get => filename; set => filename = value; }
        public string Name { get => name; set => name = value; }
        public string Extension { get => extension; set => extension = value; }
        public DataPrefix Type { get => type; set => type = value; }
    }
}
