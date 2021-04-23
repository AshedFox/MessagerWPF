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

namespace Messager
{
    /// <summary>
    /// Логика взаимодействия для MyVideoPlayer.xaml
    /// </summary>
    public partial class MyVideoPlayer : UserControl
    {
        double duration;
        double bufferedPosition;
        double currentPosition;
        MicroTimer microTimer = new MicroTimer();

        public MyVideoPlayer()
        {
            InitializeComponent();
            MainWindow.GetMainWindow().Closing += MyMediaControl_Closing;
        }

        private void MyMediaControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopVideo();
            VideoPlayer.Close();
        }

        void StopVideo()
        {
            microTimer.Stop();
            currentPosition = 0;
            bufferedPosition = 0;
            VideoPlayer.Position = TimeSpan.FromMilliseconds(0);
            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);
            TimingSlider.Value = currentPosition;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            microTimer.Start();
            VideoPlayer.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopVideo();
            VideoPlayer.Stop();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            bufferedPosition = currentPosition;
            microTimer.Stop();

            VideoPlayer.Pause();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.MediaOpened += VideoPlayer_MediaOpened;
            VideoPlayer.Source = new Uri(((VideoMessage)this.DataContext).Message);
            VideoPlayer.Stop(); 
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            duration = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            currentPosition = 0;

            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);

            TimingSlider.Minimum = 0;
            TimingSlider.Maximum = duration;
            TimingSlider.Value = 0;

            microTimer.Interval = 1000;

            VideoPlayer.MediaEnded += VideoPlayer_MediaEnded;
            TimingSlider.ValueChanged += TimingSlider_ValueChanged;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;

            microTimer.MicroTimerElapsed += MicroTimer_MicroTimerElapsed;
        }

        private void MicroTimer_MicroTimerElapsed(object sender, MicroTimer.MicroTimerEventArgs timerEventArgs)
        {
            currentPosition = bufferedPosition + timerEventArgs.ElapsedMicroseconds / 1000000;
            Console.WriteLine(currentPosition);

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
            if (Math.Round(VideoPlayer.Volume, 2) != Math.Round(e.NewValue / 100, 2))
            {
                VideoPlayer.Volume = e.NewValue / 100;
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
            bufferedPosition = newValue;
            microTimer.Stop();
            currentPosition = newValue;

            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);
            VideoPlayer.Position = TimeSpan.FromSeconds(currentPosition);

            microTimer.Start();
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopVideo();
            VideoPlayer.Stop();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopVideo();
            VideoPlayer.Close();
        }
    }
}
