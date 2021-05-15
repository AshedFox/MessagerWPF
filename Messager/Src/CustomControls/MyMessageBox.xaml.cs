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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Messager
{
    /// <summary>
    /// Логика взаимодействия для MyMessageBox.xaml
    /// </summary>
    public partial class MyMessageBox : Window
    {
        public MyMessageBox()
        {
            InitializeComponent();
        }
        static MyMessageBox myMessageBox;
        static MessageBoxResult result = MessageBoxResult.No;
        public MyMessageType MessageTitle { get; set; }
        public enum MyMessageButton
        {
            Ok,
            Yes,
            No,
            Cancel,
            Confirm

        }

        public string GetButtonText(MyMessageButton value)
        {
            return Enum.GetName(typeof(MyMessageButton), value);
        }

        public enum MyMessageType
        {
            Error,
            Info,
            Warning,
            Confirm
        }
        public static MessageBoxResult Show(string message, string title, MyMessageType type, MyMessageButton okButton, MyMessageButton noButton)
        {
            myMessageBox = new MyMessageBox();
            myMessageBox.OkButton.Content = myMessageBox.GetButtonText(okButton);
            myMessageBox.CancelButton.Content = myMessageBox.GetButtonText(noButton);
            myMessageBox.MessageTextBlock.Text = message;
            myMessageBox.TitleLabel.Content = title;

            //icon
            switch (type)
            {
                case MyMessageType.Error:
                    myMessageBox.MessageLogoImage.Source = new BitmapImage(new Uri("./Images/error.png", UriKind.Relative));
                    myMessageBox.CancelButton.Visibility = Visibility.Collapsed;
                    myMessageBox.OkButton.SetValue(Grid.ColumnSpanProperty, 2);
                    break;
                case MyMessageType.Info:                   
                    myMessageBox.MessageLogoImage.Source = new BitmapImage(new Uri("./Images/info.png", UriKind.Relative));
                    myMessageBox.CancelButton.Visibility = Visibility.Collapsed;
                    myMessageBox.OkButton.SetValue(Grid.ColumnSpanProperty, 2);
                    break;
                case MyMessageType.Warning:
                    myMessageBox.MessageLogoImage.Source = new BitmapImage(new Uri("./Images/warning.png", UriKind.Relative));
                    myMessageBox.CancelButton.Visibility = Visibility.Collapsed;
                    myMessageBox.OkButton.SetValue(Grid.ColumnSpanProperty, 2);
                    break;
                case MyMessageType.Confirm:
                    myMessageBox.MessageLogoImage.Source = new BitmapImage(new Uri("./Images/question.png", UriKind.Relative));
                    break;
            }
            myMessageBox.ShowDialog();
            return result;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;

            myMessageBox.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            myMessageBox.Close();
        }
    }
}
