using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;

namespace Messager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly string attachmentsPath = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");

        readonly Client.Client client = ClientManager.Instance.Client;

        public string currentTheme;

        private MyMediaPlayer currentPlayer;
        public MyMediaPlayer CurrentPlayer 
        { 
            get => currentPlayer;
            set 
            {
                if (currentPlayer != null)
                    currentPlayer.CloseMedia();
                currentPlayer = value;

                currentPlayer.LoadMedia(RequestAttachmentData);
            }
        }

        public void RequestAttachmentData(Attachment attachmentInfo)
        {
            client.receiveAttachment += ReceiveAttachment;
            client.SendAttachmentDataRequest(attachmentInfo);
        }

        void ReceiveAttachment(string filename, string extension, MemoryStream memoryStream)
        {
            string path = Path.Combine(attachmentsPath,
                          Path.ChangeExtension(filename, extension));

            if (!File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path,
                                                              FileMode.Create,
                                                              FileAccess.Write))
                {
                    memoryStream.WriteTo(fileStream);
                }
            }

            client.receiveAttachment -= ReceiveAttachment;
        }

        public MainWindow()
        {
            InitializeComponent();
            ThemeManager.Instance.SetupTheme();

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
