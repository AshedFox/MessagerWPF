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
        readonly long maxAttachmentSize = 52428800;
        public Dictionary<long, string> ChatUsers { get; set; } = new Dictionary<long, string>();
        public MessageCollection Messages { get; set; } = new MessageCollection();
        public List<(AttachmentInfo info, MemoryStream data)> AttachmentsToSend
        {
            get => attachmentsToSend;
            set
            {
                foreach (var item in attachmentsToSend)
                {
                    item.data.Dispose();
                }
                attachmentsToSend = value;
            }
        }

        Client.Client client;


        long chatId;
        private List<(AttachmentInfo info, MemoryStream data)> attachmentsToSend = 
            new List<(AttachmentInfo info, MemoryStream data)>();

        public ConversationPage()
        {
            this.DataContext = this;
            InitializeComponent();

            client = ClientManager.Instance.Client;
        }

        public void SetupChat(long id, MessageCollection messages, Dictionary<long, string> chatsUsers)
        {
            ChatUsers = chatsUsers;
            MessageListBox.ItemsSource = null;
            MessageListBox.Items.Clear();
            Messages = messages;
            MessageListBox.ItemsSource = Messages;
            chatId = id;
            if (messages.Count > 0)
            {
                client.SendUpdateChatMessagesRequest(id, Messages.Last().MessageId);
            }
            else
            {
                client.SendUpdateChatMessagesRequest(id, 0);
            }
            MessageListBox.SelectedIndex = MessageListBox.Items.Count - 1;
            MessageListBox.ScrollIntoView(MessageListBox.SelectedItem);
           //MessageListBox.Items.MoveCurrentToLast();
        }

        public void SetupChat(long id)
        {
            MessageListBox.ItemsSource = null;
            MessageListBox.Items.Clear();
            Messages = new MessageCollection();
            MessageListBox.ItemsSource = Messages;
            chatId = id;
            client.SendUpdateChatMessagesRequest(id, 0);
            MessageListBox.SelectedIndex = MessageListBox.Items.Count - 1;
            MessageListBox.ScrollIntoView(MessageListBox.SelectedItem);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SendMessageBox.Text) || AttachmentsToSend.Count > 0)
            {
                string message = SendMessageBox.Text;
                SendMessageBox.Clear();
                message = message.Trim();

                client.SendMessage(chatId, message, AttachmentsToSend);

                AttachmentsToSend = new List<(AttachmentInfo info, MemoryStream data)>();
                AttachmentsPanel.Children.Clear();

                SendMessageBox.Focus();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            PagesManager.Instance.MainMenuPage.UnselectChat();
        }

        private void SendCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendButton_Click(this, null);
        }

        private void AddAudioAttachment()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "MP3-audio|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
                if (fileStream.Length < maxAttachmentSize)
                {
                    if (!AttachmentsToSend.Exists(el => el.info.Filename == openFileDialog.FileName))
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);

                        AddAttachmentToList(new AttachmentInfo(openFileDialog.FileName,
                                            System.IO.Path.GetFileName(openFileDialog.FileName),
                                            System.IO.Path.GetExtension(openFileDialog.FileName),
                                            DataPrefix.Audio), memoryStream);
                    }
                    else
                    {
                        MessageBox.Show($"Данный файл уже был выбран",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Максимальный размер файла = {maxAttachmentSize / 1024 / 1024}МБ",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddVideoAttachment()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP4-file|*.mp4|MOV-file|*.mov|AVI-file|*.avi";
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
                if (fileStream.Length < maxAttachmentSize)
                {
                    if (!AttachmentsToSend.Exists(el => el.info.Filename == openFileDialog.FileName))
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);

                        AddAttachmentToList(new AttachmentInfo(openFileDialog.FileName,
                                            System.IO.Path.GetFileName(openFileDialog.FileName),
                                            System.IO.Path.GetExtension(openFileDialog.FileName),
                                            DataPrefix.Video), memoryStream);
                    }
                    else
                    {
                        MessageBox.Show($"Данный файл уже был выбран",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Максимальный размер файла = {maxAttachmentSize / 1024 / 1024}МБ",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddImageAttachment()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG-image|*.png|JPEG-image|*.jpg;*.jpeg|BMP-image|*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
                if (fileStream.Length < maxAttachmentSize)
                {
                    if (!AttachmentsToSend.Exists(el => el.info.Filename == openFileDialog.FileName))
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);

                        AddAttachmentToList(new AttachmentInfo(openFileDialog.FileName,
                                            System.IO.Path.GetFileName(openFileDialog.FileName),
                                            System.IO.Path.GetExtension(openFileDialog.FileName),
                                            DataPrefix.Image), memoryStream);
                    }
                    else
                    {
                        MessageBox.Show($"Данный файл уже был выбран",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Максимальный размер файла = {maxAttachmentSize / 1024 / 1024}МБ",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddFileAttachment()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Any file|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    if (fileStream.Length < maxAttachmentSize)
                    {
                        if (!AttachmentsToSend.Exists(el => el.info.Filename == openFileDialog.FileName))
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            fileStream.CopyTo(memoryStream);

                            AddAttachmentToList(new AttachmentInfo(openFileDialog.FileName,
                                                System.IO.Path.GetFileName(openFileDialog.FileName),
                                                System.IO.Path.GetExtension(openFileDialog.FileName),
                                                DataPrefix.File), memoryStream);
                        }
                        else
                        {
                            MessageBox.Show($"Данный файл уже был выбран",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Максимальный размер файла = {maxAttachmentSize / 1024 / 1024}МБ",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        void AddAttachmentToList(AttachmentInfo attachmentInfo, MemoryStream memoryStream)
        {
            AttachmentsToSend.Add((attachmentInfo, memoryStream));

            Grid grid = new Grid();
            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);
            grid.Margin = new Thickness(10);
            grid.Background = Brushes.White;

            Image image = new Image();
            image.Height = 24;
            Thickness margin = new Thickness(5);
            image.Margin = margin;
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
            RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
            switch (attachmentInfo.Type)
            {
                case DataPrefix.Audio:
                    image.Source = new BitmapImage(new Uri("/Resources/Images/music.png", UriKind.Relative));
                    break;
                case DataPrefix.Video:
                    image.Source = new BitmapImage(new Uri("/Resources/Images/video.png", UriKind.Relative));
                    break;
                case DataPrefix.Image:
                    image.Source = new BitmapImage(new Uri("/Resources/Images/image.png", UriKind.Relative));
                    break;
                case DataPrefix.File:
                    image.Source = new BitmapImage(new Uri("/Resources/Images/file.png", UriKind.Relative));
                    break;
                default:
                    break;
            }

            Label label = new Label
            {
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Content = attachmentInfo.Name
            };

            Image removeButton = new Image
            {
                Source = new BitmapImage(new Uri("/Resources/Images/delete2.png", UriKind.Relative)),
                Height = 20,
                Width = 20,
                Margin = new Thickness(-6),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            RenderOptions.SetBitmapScalingMode(removeButton, BitmapScalingMode.Fant);
            RenderOptions.SetEdgeMode(removeButton, EdgeMode.Aliased);
            removeButton.MouseDown += RemoveButton_MouseDown;
            removeButton.MouseEnter += RemoveButton_MouseEnter;
            removeButton.MouseLeave += RemoveButton_MouseLeave;


            Grid.SetColumn(image, 0);
            Grid.SetColumn(label, 1);
            Grid.SetColumn(removeButton, 1);
            grid.Children.Add(image);
            grid.Children.Add(label);
            grid.Children.Add(removeButton);

            grid.DataContext = attachmentInfo;

            AttachmentsPanel.Children.Add(grid);
        }

        private void RemoveButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Width = 20;
            ((Image)sender).Height = 20;
            ((Image)sender).Margin = new Thickness(-6);
        }

        private void RemoveButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Image)sender).Width = 25;
            ((Image)sender).Height = 25;
            ((Image)sender).Margin = new Thickness(-10);
        }

        private void RemoveButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int i = AttachmentsToSend.FindIndex(el => el.info == (AttachmentInfo)((Image)sender).DataContext);
            if (i >= 0) 
            {
                AttachmentsToSend[i].data.Dispose();
                AttachmentsToSend.RemoveAt(i);
                AttachmentsPanel.Children.Remove((UIElement)((Image)sender).Parent);
            }
        }

        private void AdditionalSendComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdditionalSendComboBox.SelectedIndex != -1)
            {
                if (AttachmentsToSend.Count < 5)
                {
                    switch (AdditionalSendComboBox.SelectedIndex)
                    {
                        case 0:
                            {
                                AddAudioAttachment();
                                break;
                            }
                        case 1:
                            {
                                AddVideoAttachment();
                                break;
                            }
                        case 2:
                            {
                                AddImageAttachment();
                                break;
                            }
                        case 3:
                            {
                                AddFileAttachment();
                                break;
                            }
                    }
                }
                else
                {
                    MessageBox.Show("Максимальное количество вложений - 5", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                AdditionalSendComboBox.SelectedIndex = -1;
            }
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            var data = (FileAttachment)((Control)sender).DataContext;
            if (File.Exists(data.Path))
            {
                saveFileDialog.FileName = $"{data.Name}";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string path = saveFileDialog.FileName;
                    File.Copy(data.Path, path);
                }
            }
            else MessageBox.Show("Вложение недоступно, попробуйте позже", "Ошибка", MessageBoxButton.OK);
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
                delegate{
                    ((ListBox)sender).ItemsSource = null;
                    ((ListBox)sender).Items.Clear();
                    ((ListBox)sender).ItemsSource = ((Message)((ListBox)sender).DataContext).Attachments;
                }
            );
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AttachmentsToSend = new List<(AttachmentInfo info, MemoryStream data)>();
            AttachmentsPanel.Children.Clear();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            AttachmentsToSend = new List<(AttachmentInfo info, MemoryStream data)>();
            AttachmentsPanel.Children.Clear();

            if (!PagesManager.Instance.MainMenuPage.ChatsMessages.ContainsKey(chatId)) 
            {
                PagesManager.Instance.MainMenuPage.ChatsMessages.Add(chatId, Messages);
            }
            SaveLoadSystem.Save(ClientManager.Instance.ClientInfo.id, PagesManager.Instance.MainMenuPage.ChatsMessages);
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ImageAttachment attachmentInfo = (ImageAttachment)((Image)sender).DataContext;

                if (File.Exists(attachmentInfo.Path))
                {
                    ((Image)sender).Source = (ImageSource)new ImageSourceConverter().ConvertFromString(attachmentInfo.Path);
                }
                else
                {             
                    new Thread(
                        new ThreadStart(
                        delegate
                        {
                            int i = 20;
                            while (i > 0 && !File.Exists(attachmentInfo.Path))
                            {
                                Application.Current.Dispatcher.Invoke(
                                    delegate
                                    {
                                        MainWindow.GetMainWindow().RequestAttachmentData(attachmentInfo);
                                    }
                                );
                                Thread.Sleep(500);
                                Application.Current.Dispatcher.Invoke(
                                    delegate
                                    {
                                        if (File.Exists(attachmentInfo.Path))
                                        {
                                            ((Image)sender).Source = (ImageSource)new ImageSourceConverter().ConvertFromString(attachmentInfo.Path);
                                        }
                                    }
                                );
                                i--;
                            }
                        }
                    )).Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void FileButton_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                FileAttachment attachmentInfo = (FileAttachment)((Button)sender).DataContext;

                if (!File.Exists(attachmentInfo.Path))
                {
                    new Thread(
                        new ThreadStart(
                        delegate
                        {
                            int i = 20;
                            while (i > 0 && !File.Exists(attachmentInfo.Path))
                            {
                                Application.Current.Dispatcher.Invoke(
                                    delegate
                                    {
                                        MainWindow.GetMainWindow().RequestAttachmentData(attachmentInfo);
                                    }
                                );
                                Thread.Sleep(500);
                                i--;
                            }
                        }
                    )).Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
