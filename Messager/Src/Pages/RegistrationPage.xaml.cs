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
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        MainWindow mainWindow;
        public RegistrationPage()
        {
            InitializeComponent();
            mainWindow = MainWindow.GetMainWindow();
        }

        bool RegisterClient(string login, string email, string password)
        {
            Client.Client client = ClientManager.Instance.Client;

            byte[] buff = Encoding.ASCII.GetBytes(password);

            client.SendRegistrationData(login, email, password);

            List<string> result = null;

            Thread thread = new Thread(() => client.RecieveConfirmationMessage(out result));
            thread.Start();
            if (thread.Join(30000))
            {
                if (result != null)
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
            return false;

        }

        private void RegistrateButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginField.Text != string.Empty )
            {
                if (EmailField.Text != string.Empty)
                {
                    if (PasswordField.Password.Length >= 8)
                    {
                        SplashScreen splashScreen = new SplashScreen("/Resources/loading.png");
                        splashScreen.Show(true, false);
                        if (RegisterClient(LoginField.Text, EmailField.Text,
                            EncryptionModule.EcryptPassword(PasswordField.Password)))
                        {
                            splashScreen.Close(TimeSpan.Zero);
                            PagesManager.Instance.SetMainMenuPage();
                            ErrorTextBlock.Text = "";
                        }
                    }
                    else
                    {
                        ErrorTextBlock.Text = "Пароль слишком короткий (менее 8 символов)";
                    }
                }
                else
                {
                    ErrorTextBlock.Text = "Email не указан";
                }
            }
            else
            {
                ErrorTextBlock.Text = "Имя пользователя не указано";
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            PagesManager.Instance.SetAutorizationPage();
        }

        private void RegistrateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RegistrateButton_Click(this, null);
        }
    }
}
