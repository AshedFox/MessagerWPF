using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Messager
{
    class ThemeManager
    {
        #region Singleton

        private static readonly Lazy<ThemeManager> instance =
            new Lazy<ThemeManager>(() => new ThemeManager());

        #endregion

        public ThemeManager()
        {
        }

        public static ThemeManager Instance => instance.Value;

        public static readonly string themeFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Theme.bin");

        readonly string[] themes = new string[3] {"BurgTheme","MintTheme","SunsetTheme" };

        int currentThemeIndex;
        public int CurrentThemeIndex { get => currentThemeIndex; private set => currentThemeIndex = value; }

        public void SetupTheme()
        {
            CurrentThemeIndex = 0;
            if (File.Exists(themeFile))
            {
                int.TryParse(File.ReadAllText(themeFile), out currentThemeIndex);
            }
            if (currentThemeIndex > themes.Length || currentThemeIndex < 0)
            {
                currentThemeIndex = 0;
            }
            var uri = new Uri(".\\Resources\\Themes\\" + themes[CurrentThemeIndex] + ".xaml", UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            File.WriteAllText(themeFile, CurrentThemeIndex.ToString());
        }

        public void ChangeTheme(int newThemeIndex)
        {
            CurrentThemeIndex = newThemeIndex;
            var uri = new Uri(".\\Resources\\Themes\\" + themes[CurrentThemeIndex] + ".xaml", UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            File.WriteAllText(themeFile, CurrentThemeIndex.ToString());
        }
    }
}
