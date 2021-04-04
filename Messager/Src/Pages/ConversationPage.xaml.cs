using System;
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
using NAudio;
using NAudio.Wave;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace Messager.Pages
{
    /// <summary>
    /// Логика взаимодействия для ConversationPage.xaml
    /// </summary>
    public partial class ConversationPage : Page
    {
        public MessageCollection Messages { get; } = new MessageCollection();

        Client.Client client;

        public ConversationPage()
        {
            this.DataContext = this;
            InitializeComponent();

            client = ClientManager.Instance.Client;

            client.recieveTextMessage -= RecieveTextMessage;
            client.recieveAudioMessage -= RecieveAudioMessage;

            client.recieveTextMessage += RecieveTextMessage;
            client.recieveAudioMessage += RecieveAudioMessage;
        }

        void RecieveTextMessage(string textMessage)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Messages.AddTextMessage(textMessage);
            }
            );
        }

        void RecieveAudioMessage(byte[] audioData)
        {
            string filename = Guid.NewGuid().ToString() + ".mp3";

            new FileStream($@"\{filename}", FileMode.Create).Write(audioData, 0, audioData.Length);

            Application.Current.Dispatcher.Invoke(delegate
            {
                Messages.AddAudioMessage($@"\{filename}");
            }
            );
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageBox.Text))
            {
                string message = MessageBox.Text;
                MessageBox.Clear();
                message = message.Trim();

                client.SendTextMessage(message);

                MessageBox.Focus();
            }
        }

        private void MusicButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
                MemoryStream memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);

                client.SendAudio(memoryStream.ToArray());
            }
        }

        private void PlaySoundButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MediaPlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MediaElement media = ((MediaElement)sender);
            media.Play();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendButton_Click(SendButton, null);
        }
    }
}
