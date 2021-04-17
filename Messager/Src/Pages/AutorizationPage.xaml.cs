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

            client.SendAutorizationData(login, password);

            List<string> result = null;

            Thread thread = new Thread(() => client.RecieveConfirmationMessage(out result));
            thread.Start();

            if (thread.Join(5000))
            {
                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        if (result[0] == string.Empty)
                        {
                            client.StartRecieving();

                            ClientManager.Instance.SetClientInfo(long.Parse(result[1]), result[2], result[3], result[4], result[5]);
                            
                            return client.IsAutorized;
                        }
                        else
                        {
                            ErrorTextBlock.Text = result[0];
                        }
                    }
                }
            }
            return false;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginField.Text != string.Empty)
            {
                SplashScreen splashScreen = new SplashScreen("/Resources/loading.png");
                splashScreen.Show(true, false);
                if (AuthorizeClient(LoginField.Text, EncryptionModule.EcryptPassword(PasswordField.Password)))
                {
                    splashScreen.Close(TimeSpan.Zero);
                    PagesManager.Instance.SetMainMenuPage();
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

        private void AutorizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AcceptButton_Click(this, null);
        }
    }
}
