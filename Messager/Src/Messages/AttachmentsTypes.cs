﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public abstract class Attachment
    {
        private string filename;
        private string name;
        private string extension;

        public string Filename { get => filename; 
            set 
            {
                if (filename != value)
                {
                    filename = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(filename)));

                }
            } 
        }
        public string Name { get => name; 
            set 
            { 
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(name)));

            }
        }
        public string Extension { get => extension; 
            set 
            { 
                extension = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(extension)));

            }
        }

        public string Path
        {
            get => System.IO.Path.Combine(MainWindow.attachmentsPath,
                                          System.IO.Path.ChangeExtension(Filename, Extension));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    class AudioAttachment : Attachment
    {
        public AudioAttachment(string filename, string name, string extension)
        {
            Filename = filename;
            Name = name;
            Extension = extension;
        }
    }
    class VideoAttachment : Attachment
    {
        public VideoAttachment(string filename, string name, string extension)
        {
            Filename = filename;
            Name = name;
            Extension = extension;
        }
    }
    class ImageAttachment : Attachment
    {
        public ImageAttachment(string filename, string name, string extension)
        {
            Filename = filename;
            Name = name;
            Extension = extension;
        }
    }
    class FileAttachment : Attachment
    {
        public FileAttachment(string filename, string name, string extension)
        {
            Filename = filename;
            Name = name;
            Extension = extension;
        }
    }
}
