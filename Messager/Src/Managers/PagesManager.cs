﻿using Messager.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    class PagesManager
    {
        #region Singleton

        private static readonly Lazy<PagesManager> instance =
            new Lazy<PagesManager>(() => new PagesManager());

        #endregion

        public PagesManager()
        {
            MainWindow = MainWindow.GetMainWindow();
            AutorizationPage = new AutorizationPage();
            RegistrationPage = new RegistrationPage();
            ConversationPage = new ConversationPage();
        }

        MainWindow mainWindow;
       

        public static PagesManager Instance => instance.Value;

        public AutorizationPage AutorizationPage { get => autorizationPage; private set => autorizationPage = value; }
        public RegistrationPage RegistrationPage { get => registrationPage; private set => registrationPage = value; }
        public ConversationPage ConversationPage { get => conversationPage; private set => conversationPage = value; }
        public MainWindow MainWindow { get => mainWindow; private set => mainWindow = value; }

        AutorizationPage autorizationPage;
        RegistrationPage registrationPage;
        ConversationPage conversationPage;


        public void SetAutorizationPage()
        {
            MainWindow.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            MainWindow.ResizeMode = System.Windows.ResizeMode.CanMinimize;
            MainWindow.WindowFrame.Navigate(AutorizationPage);
        }

        public void SetRegistrationPage()
        {
            MainWindow.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            MainWindow.ResizeMode = System.Windows.ResizeMode.CanMinimize;
            MainWindow.WindowFrame.Navigate(RegistrationPage);
        }

        public void SetConversationPage()
        {
            MainWindow.SizeToContent = System.Windows.SizeToContent.Manual;
            MainWindow.ResizeMode = System.Windows.ResizeMode.CanResize;
            MainWindow.WindowFrame.Navigate(ConversationPage);
        }
    }
}
