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

namespace Messager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly string attachmentsPath = 
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(attachmentsPath))
            {
                Directory.CreateDirectory(attachmentsPath);
            }

            PagesManager.Instance.SetAutorizationPage();
        }

        public static MainWindow GetMainWindow()
        {
            MainWindow mainWindow = null;

            foreach (Window window in Application.Current.Windows)
            {
                Type type = typeof(MainWindow);
                if (window != null && window.DependencyObjectType.Name == type.Name)
                {
                    mainWindow = (MainWindow)window;
                    if (mainWindow != null)
                    {
                        break;
                    }
                }
            }

            return mainWindow;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ClientManager.Instance.Client != null)
                ClientManager.Instance.Client.Disconnect();
        }
    }
}
