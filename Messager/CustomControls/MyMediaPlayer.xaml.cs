using System;
using System.Collections.Generic;
using System.IO;
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

namespace Messager
{
    /// <summary>
    /// Логика взаимодействия для MyMediaPlayer.xaml
    /// </summary>
    public partial class MyMediaPlayer : UserControl
    {
        double duration;
        double bufferedPosition;
        double currentPosition;
        MicroTimer microTimer = new MicroTimer();

        public MyMediaPlayer()
        {
            InitializeComponent();
            MainWindow.GetMainWindow().Closing += MyMediaControl_Closing;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext.GetType() == typeof(VideoAttachment))
            {
                MediaPlayer.Width = 800;
                MediaPlayer.Height = 450;
            }
        }

        public void CloseMedia()
        {
            microTimer.MicroTimerElapsed -= MicroTimer_MicroTimerElapsed;
            StopMedia();
            MediaPlayer.Close();
        }

        private void MyMediaControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseMedia();
        }

        void StopMedia()
        {
            microTimer.Stop();
            currentPosition = 0;
            bufferedPosition = 0;
            MediaPlayer.Position = TimeSpan.FromMilliseconds(0);
            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", currentPosition / 60, currentPosition % 60, duration / 60, duration % 60);
            TimingSlider.Value = 0;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GetMainWindow().CurrentPlayer = this;

            MediaPlayer.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopMedia();
            MediaPlayer.Stop();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            bufferedPosition = currentPosition;
            microTimer.Stop();

            MediaPlayer.Pause();
        }

        public void LoadMedia(Action<Attachment> requestAttachment)
        {
            MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

            if (File.Exists(((Attachment)this.DataContext).Path))
            {
                MediaPlayer.Source = new Uri(((Attachment)this.DataContext).Path);
            }
            else
            {
                requestAttachment((Attachment)this.DataContext);
                MessageBox.Show("Вложение недоступно, попробуйте позже",
                                "Ошибка",
                                MessageBoxButton.OK);
            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            microTimer.MicroTimerElapsed -= MicroTimer_MicroTimerElapsed;
            MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            TimingSlider.ValueChanged -= TimingSlider_ValueChanged;
            VolumeSlider.ValueChanged -= VolumeSlider_ValueChanged;

            duration = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            currentPosition = 0;

            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);

            TimingSlider.Minimum = 0;
            TimingSlider.Maximum = duration;
            TimingSlider.Value = 0;

            microTimer.Interval = 1000;
            microTimer.Start();

            microTimer.MicroTimerElapsed += MicroTimer_MicroTimerElapsed;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            TimingSlider.ValueChanged += TimingSlider_ValueChanged;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        }

        private void MicroTimer_MicroTimerElapsed(object sender, MicroTimer.MicroTimerEventArgs timerEventArgs)
        {
            currentPosition = bufferedPosition + timerEventArgs.ElapsedMicroseconds / 1000000;

            try
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        TimingSlider.Value = currentPosition;
                        TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);
                    });
                }
            }
            catch { }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(MediaPlayer.Volume, 2) != Math.Round(e.NewValue / 100, 2))
            {
                MediaPlayer.Volume = e.NewValue / 100;
            }
        }

        private void TimingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(e.NewValue, 2) != Math.Round(currentPosition, 2))
            {
                SetBySlider(e.NewValue);
            }
        }

        void SetBySlider(double newValue)
        {
            microTimer.Stop();
            MediaPlayer.Stop();
            bufferedPosition = newValue;
            //currentPosition = newValue;

            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);
            MediaPlayer.Position = TimeSpan.FromSeconds(currentPosition);

            MediaPlayer.Play();
            microTimer.Start();
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopMedia();
            MediaPlayer.Stop();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CloseMedia();
        }
    }
}
