using ClientServerLib;
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

namespace Messager.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        Client.Client client;
        public SettingsPage()
        {
            InitializeComponent();
            client = ClientManager.Instance.Client;
            switch (ThemeManager.Instance.CurrentThemeIndex)
            {
                case 0:
                    {
                        BurgThemeSelector.IsChecked = true;
                        break;
                    }
                case 1:
                    {
                        MintThemeSelector.IsChecked = true;
                        break;
                    }
                case 2:
                    {
                        SunsetThemeSelector.IsChecked = true;
                        break;
                    }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            PagesManager.Instance.SetMainMenuPage();
        }

        private void ChangeLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginGrid.Visibility == Visibility.Collapsed)
            {
                LoginTextBox.Text = ClientManager.Instance.ClientInfo.login;
                LoginGrid.Visibility = Visibility.Visible;
                ChangeLoginButton.Content = "Свернуть";
            }
            else
            {
                LoginGrid.Visibility = Visibility.Collapsed;
                ChangeLoginButton.Content = "Изменить";
            }
        }

        private void ChangeEmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmailGrid.Visibility == Visibility.Collapsed)
            {
                EmailTextBox.Text = ClientManager.Instance.ClientInfo.email;
                EmailGrid.Visibility = Visibility.Visible;
                ChangeEmailButton.Content = "Свернуть";
            }
            else
            {
                EmailGrid.Visibility = Visibility.Collapsed;
                ChangeEmailButton.Content = "Изменить";
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordGrid.Visibility == Visibility.Collapsed)
            {
                PasswordGrid.Visibility = Visibility.Visible;
                ChangePasswordButton.Content = "Свернуть";
            }
            else
            {
                PasswordGrid.Visibility = Visibility.Collapsed;
                ChangePasswordButton.Content = "Изменить";
            }
        }

        private void ChangeNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameGrid.Visibility == Visibility.Collapsed)
            {
                NameTextBox.Text = ClientManager.Instance.ClientInfo.name;
                NameGrid.Visibility = Visibility.Visible;
                ChangeNameButton.Content = "Свернуть";
            }
            else
            {
                NameGrid.Visibility = Visibility.Collapsed;
                ChangeNameButton.Content = "Изменить";
            }
        }

        private void SaveLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginTextBox.Text = LoginTextBox.Text.Trim();
            string error;
            if (LoginTextBox.Text == ClientManager.Instance.ClientInfo.login)
            {
                MyMessageBox.Show("Новый логин совпадает с текущим", 
                    "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                    MyMessageBox.MyMessageButton.Cancel);
            }
            else
            {
                if ((error = Validation.CheckLogin(LoginTextBox.Text)) == string.Empty)
                {
                    client.SendUpdateLoginRequest(LoginTextBox.Text);
                }
                else
                {
                    MyMessageBox.Show(error, "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                                      MyMessageBox.MyMessageButton.Cancel);
                }
            }
        }

        private void SaveEmailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailTextBox.Text = EmailTextBox.Text.Trim();
            string error;
            if (EmailTextBox.Text == ClientManager.Instance.ClientInfo.email)
            {
                MyMessageBox.Show("Новый email совпадает с текущим",
                    "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                    MyMessageBox.MyMessageButton.Cancel);
            }
            else
            {
                if ((error = Validation.CheckEmail(EmailTextBox.Text)) == string.Empty)
                {
                    client.SendUpdateEmailRequest(EmailTextBox.Text);
                }
                else
                {
                    MyMessageBox.Show(error, "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                                      MyMessageBox.MyMessageButton.Cancel);
                }
            }
        }

        private void SavePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            OldPasswordTextBox.Password = OldPasswordTextBox.Password.Trim();
            NewPasswordTextBox.Password = NewPasswordTextBox.Password.Trim();
            string error;
            if (NewPasswordTextBox.Password == OldPasswordTextBox.Password)
            {
                MyMessageBox.Show("Новый пароль совпадает со старым",
                    "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                    MyMessageBox.MyMessageButton.Cancel);
            }
            else
            {
                if ((error = Validation.CheckPassword(OldPasswordTextBox.Password)) == string.Empty)
                {
                    if ((error = Validation.CheckPassword(NewPasswordTextBox.Password)) == string.Empty)
                    {
                        client.SendUpdatePasswordRequest(
                            EncryptionModule.EcryptPassword(OldPasswordTextBox.Password),
                            EncryptionModule.EcryptPassword(NewPasswordTextBox.Password));
                        return;
                    }
                }
                MyMessageBox.Show(error, "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                                    MyMessageBox.MyMessageButton.Cancel);
            }

        }

        private void SaveNameButton_Click(object sender, RoutedEventArgs e)
        {
            NameTextBox.Text = NameTextBox.Text.Trim();
            string error;
            if (NameTextBox.Text == ClientManager.Instance.ClientInfo.name)
            {
                MyMessageBox.Show("Новый никнейм совпадает с текущим",
                    "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                    MyMessageBox.MyMessageButton.Cancel);
            }
            else
            {
                if ((error = Validation.CheckName(NameTextBox.Text)) == string.Empty)
                {
                    client.SendUpdateNameRequest(NameTextBox.Text);
                }
                else
                {
                    MyMessageBox.Show(error, "Ошибка", MyMessageBox.MyMessageType.Error, MyMessageBox.MyMessageButton.Yes,
                                      MyMessageBox.MyMessageButton.Cancel);
                }
            }
        }

        public void ShowConfirmationMessage(UpdateType type, bool isSuccessful, string newValue)
        {
            string message = string.Empty;
            switch (type)
            {
                case UpdateType.Email:
                    message = "Email " + (isSuccessful ? "успешно изменён" : "не удалось изменить");
                    ClientManager.Instance.ClientInfo.email = newValue;
                    break;
                case UpdateType.Name:
                    message = "Никнейм " + (isSuccessful ? "успешно изменён" : "не удалось изменить");

                    foreach (var item in PagesManager.Instance.MainMenuPage.ChatsUsers)
                    {
                        if (item.Value.ContainsKey(ClientManager.Instance.ClientInfo.id))
                        {
                            item.Value[ClientManager.Instance.ClientInfo.id] = newValue;
                        }
                    }
                    ClientManager.Instance.ClientInfo.name = newValue;
                    /*                    foreach (var item in PagesManager.Instance.MainMenuPage.ChatsMessages.Values)
                                        {
                                            item.UpdateName(ClientManager.Instance.ClientInfo.id, newValue);
                                        }*/
                    //ClientManager.Instance.ClientInfo.name = newValue;
                    break;
                case UpdateType.Login:
                    message = "Логин " + (isSuccessful ? "успешно изменён" : "не удалось изменить");
                    ClientManager.Instance.ClientInfo.login = newValue;
                    break;
                case UpdateType.Password:
                    message = "Пароль " + (isSuccessful ? "успешно изменён" : "не удалось изменить");
                    break;
            }
            Application.Current.Dispatcher.Invoke(delegate
            {
                MyMessageBox.Show(message, isSuccessful ? "Успех" : "Ошибка",
                    isSuccessful ? MyMessageBox.MyMessageType.Info : MyMessageBox.MyMessageType.Error,
                    MyMessageBox.MyMessageButton.Yes,
                    MyMessageBox.MyMessageButton.Cancel);
            }
            );

        }

        private void BurgThemeSelector_Checked(object sender, RoutedEventArgs e)
        {
            ChangeTheme(0);
        }

        private void MintThemeSelector_Checked(object sender, RoutedEventArgs e)
        {
            ChangeTheme(1);
        }

        private void SunsetThemeSelector_Checked(object sender, RoutedEventArgs e)
        {
            ChangeTheme(2);
        }

        void ChangeTheme(int newThemeIndex)
        {
            ThemeManager.Instance.ChangeTheme(newThemeIndex);
        }
    }
}
