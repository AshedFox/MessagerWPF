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
    /// Логика взаимодействия для AutorizationPage.xaml
    /// </summary>
    public partial class AutorizationPage : Page
    {
        MainWindow mainWindow;

        public AutorizationPage()
        {
            InitializeComponent();
            mainWindow = MainWindow.GetMainWindow();
        }

        public bool AuthorizeClient(string login, string password)
        {
            Client.Client client = ClientManager.Instance.Client;

            byte[] buff = Encoding.UTF8.GetBytes(password);

            client.SendAutorizationData(login, password);

            string result = null;

            Thread thread = new Thread(() => client.RecieveConfirmationMessage(out result));
            thread.Start();
            thread.Join(5000);

            if (result == string.Empty)
            {
                client.StartRecieving();
                return client.IsAutorized;

            }
            else
            {
                ErrorTextBlock.Text = result;
                return false;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginField.Text != string.Empty)
            {
                if (AuthorizeClient(LoginField.Text, EncryptionModule.EcryptPassword(PasswordField.Password)))
                {
                    PagesManager.Instance.SetConversationPage();
                    ErrorTextBlock.Text = "";
                }
            }
            else
            {
                ErrorTextBlock.Text = "Логин или email не введён";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            PagesManager.Instance.SetRegistrationPage();
        }
    }
}
