﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using ClientServerLib;

namespace Messager.Pages
{
    /// <summary>
    /// Логика взаимодействия для ConversationPage.xaml
    /// </summary>
    public partial class ConversationPage : Page
    {

        public MessageCollection Messages { get; } = new MessageCollection();

        Client.Client client;

        long chatId;

        public ConversationPage()
        {
            this.DataContext = this;
            InitializeComponent();

            client = ClientManager.Instance.Client;

            client.recieveMessage += RecieveMessage;

        }

        void RecieveMessage(MessageInfo messageInfo)
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                {
                    Messages.AddMessage(messageInfo);
                }
            );
        }

        /*        void RecieveAudioMessage(AttachmentInfo messageInfo, MemoryStream data)
                {
                    string path = System.IO.Path.Combine(MainWindow.attachmentsPath,
                                                         System.IO.Path.ChangeExtension(messageInfo.Filename,
                                                                                        messageInfo.Extension));

                    if (!File.Exists(path))
                    {
                        using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            data.WriteTo(fileStream);
                        }
                    }

                    Application.Current.Dispatcher.Invoke(
                        delegate
                        {
                            Messages.AddAudioMessage(messageInfo);
                        }
                    );
                }*/


        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SendMessageBox.Text))
            {
                string message = SendMessageBox.Text;
                SendMessageBox.Clear();
                message = message.Trim();

                client.SendMessage(chatId, message, 0);

                SendMessageBox.Focus();
            }
        }

        public void SetupChat(long id)
        {
            MessageListBox.ItemsSource = null;
            MessageListBox.Items.Clear();
            Messages.Clear();
            MessageListBox.ItemsSource = Messages;
            chatId = id;
            //client.IsReadingAvailable = false;
            client.SendGetAllMessagesByChatRequest(id);
            /*
            List<string> result = null;

            Thread thread = new Thread(() => client.RecieveConfirmationMessage(out result));

            thread.Start();


            if (thread.Join(5000))
            {
                if (result != null)
                {
                    MessageListBox.ItemsSource = null;
                    MessageListBox.Items.Clear();
                    Messages.Clear();
                    MessageListBox.ItemsSource = Messages;
                    if (result.Count > 1)
                    {
                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            string[] messageData = result[i + 1].Split('\n');
                            Messages.AddMessage(new MessageInfo(long.Parse(messageData[0]),
                                                                messageData[1],
                                                                messageData[2],
                                                                messageData[3]));
*//*                            Messages.Add(new TextMessage()
                            {
                                MessageInfo = new MessageInfo(
                                    long.Parse(messageData[0]),
                                    messageData[1],
                                    messageData[2],
                                    messageData[3]
                                )
                            });*//*
                        }
                    }
                    else
                    {
                        // сообщение об ошибке
                    }
                }
            }
            client.IsReadingAvailable = true;*/
        }

        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            PagesManager.Instance.MainMenuPage.UnselectChat();
        }

        private void SendCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendButton_Click(this, null);
        }

        private void SendMusic()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "MP3-audio|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
                if (fileStream.Length < 536870912)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);

                    client.SendMessage(
                        chatId, "", 1,
                        DataPrefix.Audio,
                        System.IO.Path.GetFileName(openFileDialog.FileName),
                        System.IO.Path.GetExtension(openFileDialog.FileName),
                        memoryStream.ToArray()
                    );
                    memoryStream.Dispose();
                }
                else
                {
                    MessageBox.Show("Max file size = 512MB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SendVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP4-file|*.mp4|MOV-file|*.mov|AVI-file|*.avi";
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
                if (fileStream.Length < 536870912)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);

                    client.SendMessage(
                        chatId, "", 1,
                        DataPrefix.Video,
                        System.IO.Path.GetFileName(openFileDialog.FileName),
                        System.IO.Path.GetExtension(openFileDialog.FileName),
                        memoryStream.ToArray()
                    );
                    memoryStream.Dispose();
                }
                else
                {
                    MessageBox.Show("Max file size = 512MB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SendImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap-image|*.bmp|PNG-image|*.png|JPEG-image|*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
                if (fileStream.Length < 536870912)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);

                    client.SendMessage(
                        chatId, "", 1,
                        DataPrefix.Image,
                        System.IO.Path.GetFileName(openFileDialog.FileName),
                        System.IO.Path.GetExtension(openFileDialog.FileName),
                        memoryStream.ToArray()
                    );
                    memoryStream.Dispose();
                }
                else
                {
                    MessageBox.Show("Max file size = 512MB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SendFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Any file|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    if (fileStream.Length < 536870912)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);

                        client.SendMessage(
                            chatId, "", 1,
                            DataPrefix.File,
                            System.IO.Path.GetFileName(openFileDialog.FileName),
                            System.IO.Path.GetExtension(openFileDialog.FileName),
                            memoryStream.ToArray()
                        );
                        memoryStream.Dispose();
                    }
                    else
                    {
                        MessageBox.Show("Max file size = 512MB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AdditionalSendComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdditionalSendComboBox.SelectedIndex != -1)
            {
                switch (AdditionalSendComboBox.SelectedIndex)
                {
                    case 0:
                        {
                            SendMusic();
                            break;
                        }
                    case 1:
                        {
                            SendVideo();
                            break;
                        }
                    case 2:
                        {
                            SendImage();
                            break;
                        }
                    case 3:
                        {
                            SendFile();
                            break;
                        }
                }
                AdditionalSendComboBox.SelectedIndex = -1;
            }
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            var data = (FileAttachment)((Control)sender).DataContext;
            saveFileDialog.FileName = $"{data.Name}";

            if (saveFileDialog.ShowDialog() == true)
            {
                string path = saveFileDialog.FileName;
                File.Copy(data.Path, path);
            }
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((ListBox)sender).Items.Clear();
            ((ListBox)sender).ItemsSource = ((Message)((ListBox)sender).DataContext).Attachments;
        }
    }
}
