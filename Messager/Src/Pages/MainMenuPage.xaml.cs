using ClientServerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Messager.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainMenuPage.xaml
    /// </summary>
    public partial class MainMenuPage : Page
    {
        public bool IsSearch { get; private set; } = false;

        public ContactsCollection Contacts { get; } = new ContactsCollection();
        public ContactsCollection SearchResults { get; } = new ContactsCollection();

        readonly Client.Client client = ClientManager.Instance.Client;

        public Dictionary<long, MessageCollection> ChatsMessages { get; private set; }
        public Dictionary<long, Dictionary<long, string>> ChatsUsers { get; private set; }

        long userId;

        public MainMenuPage()
        {
            this.DataContext = this;
            InitializeComponent();
            client.receiveMessage += RecieveMessage;
        }

        private void MainMenuPage_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            SaveLoadSystem.Save(userId, ChatsMessages);
        }


        public void UserAutorized(long userId)
        {
            this.userId = userId;
            client.receiveMessagesUpdate -= ReceiveUpdateData;
            client.recieveChatUsers -= RecieveChatUsers;
            Loaded -= MainMenuPage_Loaded;
            MainWindow.GetMainWindow().Closing -= MainMenuPage_Closing;

            RequestContacts();
            ChatsMessages = SaveLoadSystem.Load(userId);
            //SetContactsLastMessages();

            client.receiveMessagesUpdate += ReceiveUpdateData;
            client.recieveChatUsers += RecieveChatUsers;
            Loaded += MainMenuPage_Loaded;
            MainWindow.GetMainWindow().Closing += MainMenuPage_Closing;
        }

        void SetContactsLastMessages()
        {
            foreach (var item in Contacts)
            {
                if (ChatsMessages.TryGetValue(item.Id, out MessageCollection messages))
                {
                    item.LastMessage = messages.Last().MessageText == string.Empty ? "<attachment>" : messages.Last().MessageText;
                }
            }
        }

        void RenewLastContactMessage(long chatId)
        {
            if (ChatsMessages.TryGetValue(chatId, out MessageCollection messages))
            {
                if (Contacts.ToList().Exists(el => el.Id == chatId))
                {
                    var contact = Contacts.First(el => el.Id == chatId);
                    if (messages.Last().MessageText == "")
                        contact.LastMessage = "<Attachment>";
                    else
                        contact.LastMessage = messages.Last().MessageText;
                }
            }
        }

        private void MainMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(() =>InitializeChats()).Start();
        }

        void InitializeChats()
        {
            InitializeChatsUsers();
            InitializeChatsMessages();
        }

        void InitializeChatsUsers()
        {
            ChatsUsers = new Dictionary<long, Dictionary<long, string>>();
            foreach (var item in Contacts)
            {
                client.SendGetChatUsersRequest(item.Id);
            }
        }

        void InitializeChatsMessages()
        {
            foreach (var item in Contacts)
            {
                if (!ChatsMessages.ContainsKey(item.Id))
                {
                    ChatsMessages.Add(item.Id, new MessageCollection());
                }
            }

            UpdateMessages();
        }

        private void UpdateMessages()
        {
            foreach (var item in ChatsMessages)
            {
                if (item.Value.Count == 0)
                    client.SendGetNewChatMessagesRequest(item.Key, 0);
                else 
                    client.SendGetNewChatMessagesRequest(item.Key, item.Value.Last().MessageId);
            }
        }

        public void RecieveMessage(MessageInfo messageInfo) 
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                {
                    if (ChatsMessages.ContainsKey(messageInfo.ChatId))
                    {
                        ChatsMessages[messageInfo.ChatId].AddMessage(messageInfo);
                        RenewLastContactMessage(messageInfo.ChatId);
                    }
                }
            );
        }

        public void RecieveChatUsers(long chatId, Dictionary<long, string> chatUsers)
        {
            ChatsUsers.Add(chatId, chatUsers);
        }

        public void ReceiveUpdateData(MessageInfo messageInfo)
        {
            ChatsMessages[messageInfo.ChatId].AddMessage(messageInfo);
        }

        public void RequestContacts()
        {
            if (ClientManager.Instance.ClientInfo != null)
            {
                client.IsReadingAvailable = false;

                client.SendGetAllUserContactsRequest();

                List<string> result = null;

                Thread thread = new Thread(() => client.ReceiveConfirmationMessage(out result));

                thread.Start();

                if (thread.Join(30000))
                {
                    if (result != null)
                    {
                        if (result.Count > 1)
                        {
                            for (int i = 1; i < result.Count; i++)
                            {
                                string[] contactData = result[i].Split('\n');

                                ContactInfo contactInfo = new ContactInfo(long.Parse(contactData[0]), contactData[1]);

                                if (!Contacts.ToList().Exists(el => el.Id == contactInfo.id))
                                {
                                    Contacts.AddContact(contactInfo.id, contactInfo.name);
                                }
                                else
                                {
                                    Contacts.FirstOrDefault(el => el.Id == contactInfo.id).Name = contactInfo.name;
                                }
                            }
                        }
                        else
                        {
                            // сообщение об ошибке
                        }
                    }
                }
                client.IsReadingAvailable = true;
            }
        }

        bool AddChat(long user2Id)
        {
            bool isAdded = false;
            client.IsReadingAvailable = false;
            client.SendAddChatRequest(user2Id);

            List<string> result = null;

            Thread thread = new Thread(() => client.ReceiveConfirmationMessage(out result));

            thread.Start();


            if (thread.Join(30000))
            {
                if (result != null)
                {
                    if (result.Count > 1)
                    {
                        string[] contactData = result[1].Split('\n');

                        ContactInfo contactInfo = new ContactInfo(long.Parse(contactData[0]), contactData[1]);

                        if (!Contacts.ToList().Exists(el => el.Id == contactInfo.id))
                        {
                            //ClientManager.Instance.AvalibleContacts.Add(contactInfo);

                            Contacts.AddContact(contactInfo.id, contactInfo.name);
                            isAdded = true;
                        }
                    }
                    else
                    {
                        // сообщение об ошибке
                    }
                }
            }
            client.IsReadingAvailable = true;
            return isAdded;
        }

        void SearchContacts(string namePattern)
        {
            Contacts.ToList().ForEach(el =>
                {
                    if (el.Name.ToLower().Contains(namePattern.ToLower())) 
                    {
                        SearchResults.AddContact(el.Id, el.Name);
                    }
                }
            );
            client.IsReadingAvailable = false;

            client.SendSearchContactsRequest(namePattern);

            List<string> result = null;

            Thread thread = new Thread(() => client.ReceiveConfirmationMessage(out result));

            thread.Start();

            if (thread.Join(30000))
            {
                if (result != null)
                {
                    if (result.Count > 1)
                    {
                        for (int i = 1; i < result.Count; i++)
                        {
                            string[] contactData = result[i].Split('\n');

                            ContactInfo contactInfo = new ContactInfo(long.Parse(contactData[0]), contactData[1]);

                            SearchResults.AddContact(contactInfo.id, contactInfo.name);
                        }
                    }
                    else
                    {
                        // сообщение об ошибке
                    }
                }
            }
            client.IsReadingAvailable = true;
        }

        void DeleteContact(long chatId)
        {
            client.SendDeleteContactRequest(chatId);
            Contacts.RemoveContact(chatId);
            ChatsMessages.Remove(chatId);
            SaveLoadSystem.Save(userId, ChatsMessages);
        }

        private void ContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsListBox.ItemsSource == Contacts && e.AddedItems.Count == 1)
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    PagesManager.Instance.ConversationPage.ConversationNameLabel.Content =
                        ((Contact)e.AddedItems[0]).Name;
                    long id = ((Contact)e.AddedItems[0]).Id;
                    if (!ChatsMessages.ContainsKey(id))
                    {
                        ChatsMessages.Add(id, new MessageCollection());
                    }
                    if (ChatsUsers.ContainsKey(id))
                    {
                        PagesManager.Instance.ConversationPage.SetupChat(id, ChatsMessages[id], ChatsUsers[id]);
                    }
                    else
                    {
                        PagesManager.Instance.ConversationPage.SetupChat(id, ChatsMessages[id], null);
                    }
                    ConversationFrame.Navigate(PagesManager.Instance.ConversationPage);

                });
            }
            else if (ContactsListBox.ItemsSource == SearchResults && e.AddedItems.Count == 1)
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    PagesManager.Instance.ConversationPage.ConversationNameLabel.Content =
                        SearchResults[ContactsListBox.Items.IndexOf(e.AddedItems[0])].Name;

                    if (AddChat(((Contact)e.AddedItems[0]).Id))
                    {
                        long id = Contacts.Last().Id;
                        ChatsMessages.Add(id, new MessageCollection());
                        ((Contact)e.AddedItems[0]).Id = id;
                        if (ChatsUsers.ContainsKey(id))
                        {
                            PagesManager.Instance.ConversationPage.SetupChat(id, ChatsMessages[id], ChatsUsers[id]);
                        }
                        else
                        {
                            PagesManager.Instance.ConversationPage.SetupChat(id, ChatsMessages[id], null);
                        }
                    }
                    else
                    {
                        long id = ((Contact)e.AddedItems[0]).Id;
                        if (!ChatsMessages.ContainsKey(id))
                        {
                            ChatsMessages.Add(id, new MessageCollection());
                            PagesManager.Instance.ConversationPage.SetupChat(id);
                        }
                        if (ChatsUsers.ContainsKey(id))
                        {
                            PagesManager.Instance.ConversationPage.SetupChat(id, ChatsMessages[id], ChatsUsers[id]);
                        }
                        else
                        {
                            PagesManager.Instance.ConversationPage.SetupChat(id, ChatsMessages[id], null);
                        }
                    }

                    ConversationFrame.Navigate(PagesManager.Instance.ConversationPage);
                });
            }
        }

        public void UnselectChat()
        {
            ConversationFrame.Navigate(null);
            ContactsListBox.UnselectAll();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text != string.Empty)
            {
                CancelButton.Visibility = Visibility.Visible;
                ContactsListBox.ItemsSource = SearchResults;
                SearchResults.Clear();
                IsSearch = true;
                SearchContacts(SearchTextBox.Text);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            PagesManager.Instance.SetSettingsPage();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ContactsListBox.ItemsSource = Contacts;
            SearchResults.Clear();
            IsSearch = false;
            CancelButton.Visibility = Visibility.Hidden;
            SearchTextBox.Clear();
        }

        private void DeleteContactImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (MyMessageBox.Show(
                 "Вы уверены, что хотите удалить контакт из списка?",
                 "Удалить контакт",
                 MyMessageBox.MyMessageType.Confirm,
                 MyMessageBox.MyMessageButton.Yes,
                 MyMessageBox.MyMessageButton.No) == MessageBoxResult.Yes)
            {
                DeleteContact(((Contact)((Image)sender).DataContext).Id);
            } 
        }

        private void DeleteContactImage_MouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            ((Image)sender).Width = 25;
            ((Image)sender).Height = 25;
            ((Image)sender).Margin = (Thickness)new ThicknessConverter().ConvertFromString( "-10, -15");
        }

        private void DeleteContactImage_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            ((Image)sender).Width = 20;
            ((Image)sender).Height = 20;
            ((Image)sender).Margin = (Thickness)new ThicknessConverter().ConvertFromString("-7, -11");
        }
    }
}
