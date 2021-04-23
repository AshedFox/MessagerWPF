using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Messager
{
    /// <summary>
    /// Логика взаимодействия для MyMediaControl.xaml
    /// </summary>
    public partial class MyAudioPlayer : UserControl
    {
        MediaPlayer mediaPlayer;
        double duration;
        double bufferedPosition;
        double currentPosition;
        //Timer timer = new Timer();
        MicroTimer microTimer = new MicroTimer();

        
        public MyAudioPlayer()
        {
            InitializeComponent();

            MainWindow.GetMainWindow().Closing += MyMediaControl_Closing;
        }

        private void MyMediaControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopAudio();
            mediaPlayer.Close();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopAudio();
            mediaPlayer.Close();
        }

        private void TimingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(e.NewValue, 2) != Math.Round(currentPosition,2))
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
            mediaPlayer.Position = TimeSpan.FromSeconds(currentPosition);

            microTimer.Start();
        }

        void StopAudio()
        {
            microTimer.Stop();
            currentPosition = 0;
            bufferedPosition = 0;
            mediaPlayer.Position = TimeSpan.FromMilliseconds(0);
            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);
            TimingSlider.Value = currentPosition;
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            StopAudio();
            mediaPlayer.Stop();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            microTimer.Start();
            mediaPlayer.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopAudio();
            mediaPlayer.Stop();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            bufferedPosition = currentPosition;
            microTimer.Stop();

            mediaPlayer.Pause();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.Open(new Uri(((AudioMessage)this.DataContext).Message));
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            duration = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            currentPosition = 0;

            TimingLabel.Content = string.Format("{0:00}:{1:00}/{2:00}:{3:00}", (int)currentPosition / 60, (int)currentPosition % 60, (int)duration / 60, (int)duration % 60);

            TimingSlider.Minimum = 0;
            TimingSlider.Maximum = duration;
            TimingSlider.Value = 0;

            microTimer.Interval = 1000;

            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            TimingSlider.ValueChanged += TimingSlider_ValueChanged;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;

            microTimer.MicroTimerElapsed += MicroTimer_MicroTimerElapsed;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(mediaPlayer.Volume, 2) != Math.Round(e.NewValue / 100, 2)) 
            {
                mediaPlayer.Volume = e.NewValue / 100;
            }
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
    }
}
