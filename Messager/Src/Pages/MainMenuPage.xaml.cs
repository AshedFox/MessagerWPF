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
        public ContactsCollection Contacts { get; } = new ContactsCollection();
        public ContactsCollection SearchResults { get; } = new ContactsCollection();

        Client.Client client = ClientManager.Instance.Client;

        public MainMenuPage()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public void RequestContacts()
        {
            if (ClientManager.Instance.ClientInfo != null)
            {
                client.IsReadingAvailable = false;

                client.SendGetAllContactsRequest();

                List<string> result = null;

                Thread thread = new Thread(() => client.RecieveConfirmationMessage(out result));

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

                                if (!ClientManager.Instance.AvalibleContacts.Exists(el => el.id == contactInfo.id))
                                {
                                    ClientManager.Instance.AvalibleContacts.Add(contactInfo);

                                    Contacts.AddContact(contactInfo.id, contactInfo.name);
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

        void AddChat(long user2Id)
        {
            client.IsReadingAvailable = false;
            client.SendAddchatRequest(user2Id);

            List<string> result = null;

            Thread thread = new Thread(() => client.RecieveConfirmationMessage(out result));

            thread.Start();


            if (thread.Join(5000))
            {
                if (result != null)
                {
                    if (result.Count > 1)
                    {
                        string[] contactData = result[1].Split('\n');

                        ContactInfo contactInfo = new ContactInfo(long.Parse(contactData[0]), contactData[1]);

                        if (!ClientManager.Instance.AvalibleContacts.Exists(el => el.id == contactInfo.id))
                        {
                            ClientManager.Instance.AvalibleContacts.Add(contactInfo);

                            Contacts.AddContact(contactInfo.id, contactInfo.name);
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

        void SearchContacts(string namePattern)
        {
            client.IsReadingAvailable = false;

            client.SendSearchRequest(namePattern);

            List<string> result = null;

            Thread thread = new Thread(() => client.RecieveConfirmationMessage(out result));

            thread.Start();

            if (thread.Join(5000))
            {
                if (result != null)
                {
                    ContactsListBox.ItemsSource = SearchResults;
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

        private void ContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsListBox.ItemsSource == Contacts && e.AddedItems.Count == 1)
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    PagesManager.Instance.ConversationPage.ConversationNameLabel.Content =
                        Contacts[ContactsListBox.Items.IndexOf(e.AddedItems[0])].Name;
                    PagesManager.Instance.ConversationPage.SetupChat(
                         Contacts[ContactsListBox.Items.IndexOf(e.AddedItems[0])].Id);
                    ConversationFrame.Navigate(PagesManager.Instance.ConversationPage);

                });
            }
            else if (ContactsListBox.ItemsSource == SearchResults && e.AddedItems.Count == 1)
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    PagesManager.Instance.ConversationPage.ConversationNameLabel.Content =
                        SearchResults[ContactsListBox.Items.IndexOf(e.AddedItems[0])].Name;
                    
                    AddChat(SearchResults[ContactsListBox.Items.IndexOf(e.AddedItems[0])].Id); 
                    PagesManager.Instance.ConversationPage.SetupChat(
                        SearchResults[ContactsListBox.Items.IndexOf(e.AddedItems[0])].Id);

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
                CancelButton.IsEnabled = true;
                SearchResults.Clear();
                SearchContacts(SearchTextBox.Text);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ContactsListBox.ItemsSource = Contacts;
            SearchResults.Clear();
            CancelButton.IsEnabled = false;
            SearchTextBox.Clear();
        }
    }
}
