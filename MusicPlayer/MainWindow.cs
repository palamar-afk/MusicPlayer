using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;



namespace MusicPlayer
{
    public partial class MainWindow
    {
        private string _pathToFile = String.Empty; 
        private readonly MediaPlayer _mainPlayer = new MediaPlayer();
        private double _totalDuration;
        private readonly DispatcherTimer _timerForSliderPosition = new DispatcherTimer();
        private readonly OpenFileDialog _fileDialog = new OpenFileDialog();
        private int _indexOfCurrentTrack;
        private bool _isTrackPaused = true;
        private bool _isListRepeated;
        private bool _isTrackRepeated;
        
        
        public MainWindow()
        {
            InitializeComponent();
            SetPropertiesForSliders();
            FormatButtons();
            ButtonChooseMusic_OnClick(null, null);
        }
        
        private void SetPropertiesForSliders()
        {
            SliderVolume.Value = _mainPlayer.Volume = 0.1;
            SliderBalance.Value = _mainPlayer.Balance = 0;
        }
        private void FormatButtons()
        {
            for(int i = 0; i < MainControlsWrapPanel.Children.Count; i++)
            {
                if (MainControlsWrapPanel.Children[i] is Button)
                {
                    Button button = (Button)MainControlsWrapPanel.Children[i];
                    button.Width = 200;
                    button.Margin = new Thickness(0, 0, 0, 5);
                }
            }
        }

        private void FillTextNameOfTrack()
        {
            CurrentTrackTextBox.Text = "Track: " + _fileDialog.SafeFileNames[_indexOfCurrentTrack];
        }
        private void ButtonChooseMusic_OnClick(object sender, RoutedEventArgs e)
        {
            _fileDialog.Multiselect = true;
            _mainPlayer.Stop();
            _indexOfCurrentTrack = 0;
            _fileDialog.ShowDialog();
            _pathToFile = _fileDialog.FileNames[_indexOfCurrentTrack];
            _mainPlayer.Open(new Uri(_pathToFile));
            
            FillPlayList();
            
            FillTextNameOfTrack();
            
            PaintTileOfCurrentTrack();
            
            _mainPlayer.MediaOpened += (s, ev) =>
            {
                _totalDuration = _mainPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SliderPosition.Maximum = _totalDuration;
            };
            
            _mainPlayer.MediaEnded += (s, ev) =>
            {
                if (_isTrackRepeated)
                {
                    _mainPlayer.Stop();
                    _mainPlayer.Play();
                    return;
                }
                ButtonNextTrack_OnClick(null, null);
            };
        }
        
        private void ButtonPlayMusic_OnClick(object sender, RoutedEventArgs e)
        {
            _mainPlayer.Volume = SliderVolume.Value;
            _mainPlayer.Play();
            _timerForSliderPosition.Tick += SetSliderPositionValue;
            _timerForSliderPosition.Start();
            _isTrackPaused = false;
        }

        private void ButtonPauseMusic_OnClick(object sender, RoutedEventArgs e)
        {
            _mainPlayer.Pause();
            _timerForSliderPosition.Stop();
            _isTrackPaused = true;
        }

        private void ButtonStopMusic_OnClick(object sender, RoutedEventArgs e)
        {
            _mainPlayer.Stop();
            SliderPosition.Value = 0;
            _timerForSliderPosition.Stop();
            _isTrackPaused = true;
        }
        
        private void Slider_VolumeChanged(object sender, RoutedEventArgs e)
        {
            _mainPlayer.Volume = ((Slider)sender).Value;
        }

        private void Slider_BalanceChanged(object sender, RoutedEventArgs e)
        {
            _mainPlayer.Balance = ((Slider)sender).Value;
        }
        
        private void ButtonGoBack_OnClick(object sender, RoutedEventArgs e)
        {
            _timerForSliderPosition.Stop();
            SliderPosition.Value = 0;
            _mainPlayer.Position = TimeSpan.Zero;
            _mainPlayer.Play();
            _timerForSliderPosition.Start();
        }
        
        private void SliderPosition_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _timerForSliderPosition.Stop();
        }
        private void SliderPosition_OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            _mainPlayer.Position = TimeSpan.FromSeconds(SliderPosition.Value);
            if (!_isTrackPaused)
            {
                _mainPlayer.Play();
            }
            _timerForSliderPosition.Start();
            TextAudioDuration.Text = TimeSpan.FromSeconds(Math.Round(_mainPlayer.Position.TotalSeconds)).ToString();
        }

        private void SetSliderPositionValue(object sender, EventArgs e)
        {
            SliderPosition.Value = _mainPlayer.Position.TotalSeconds;
            TextAudioDuration.Text = TimeSpan.FromSeconds(Math.Round(SliderPosition.Value)).ToString();
        }

        private void ClearTileOfTrack()
        {
            ((ListBoxItem) PlayListBox.Items[_indexOfCurrentTrack]).Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }
        
        private void PaintTileOfCurrentTrack()
        {
            ((ListBoxItem) PlayListBox.Items[_indexOfCurrentTrack]).Background = new SolidColorBrush(Color.FromRgb(159, 226, 134));
        }

        private void ButtonPreviousTrack_OnClick(object sender, RoutedEventArgs e)
        {
            if (_indexOfCurrentTrack == 0)
            {
                return;
            }
            _mainPlayer.Stop();
            SliderPosition.Value = 0;

            ClearTileOfTrack();
            
            _indexOfCurrentTrack--;
            _pathToFile = _fileDialog.FileNames[_indexOfCurrentTrack];
            
            FillTextNameOfTrack();
            
            _mainPlayer.Open(new Uri(_pathToFile));
            PaintTileOfCurrentTrack();

            if (!_isTrackPaused)
            {
                _mainPlayer.Play();
            }
        }

        private void ButtonNextTrack_OnClick(object sender, RoutedEventArgs e)
        {
            if (_indexOfCurrentTrack + 1 == _fileDialog.FileNames.Length )
            {
                if (!_isListRepeated)
                {
                    return;
                }
                
                ClearTileOfTrack();
                _indexOfCurrentTrack = 0;
                _mainPlayer.Stop();
                _pathToFile = _fileDialog.FileNames[_indexOfCurrentTrack];
                FillTextNameOfTrack();
                _mainPlayer.Open(new Uri(_pathToFile));
                PaintTileOfCurrentTrack();
                _mainPlayer.Play();
                return;
            }
            _mainPlayer.Stop();
            SliderPosition.Value = 0;
            ClearTileOfTrack();
            _indexOfCurrentTrack++;
            PaintTileOfCurrentTrack();
            _pathToFile = _fileDialog.FileNames[_indexOfCurrentTrack];
            FillTextNameOfTrack();
            _mainPlayer.Open(new Uri(_pathToFile));
            
            if (!_isTrackPaused)
            {
                _mainPlayer.Play();
            }
        }

        private void FillPlayList()
        {
            PlayListBox.Items.Clear();
            foreach (string fileName in _fileDialog.SafeFileNames)
            { 
                PlayListBox.Items.Add(new ListBoxItem {Content = fileName});
                ((ListBoxItem) PlayListBox.Items[^1]).MouseDoubleClick += RandomChoseTrackAction;
            }
        }
        
        private void RandomChoseTrackAction(object sender, MouseEventArgs e)
        {
            ListBoxItem item = (ListBoxItem) sender;
            
            if (_indexOfCurrentTrack == PlayListBox.Items.IndexOf(item))
            {
                return;
            }
            
            _mainPlayer.Stop();
            
            ClearTileOfTrack();
            _indexOfCurrentTrack = PlayListBox.Items.IndexOf(item);
            PaintTileOfCurrentTrack();

            _pathToFile = _fileDialog.FileNames[_indexOfCurrentTrack];
            FillTextNameOfTrack();
            _mainPlayer.Open(new Uri(_pathToFile));
            SliderPosition.Value = 0;
            if (!_isTrackPaused)
            {
                _mainPlayer.Play();
            }
        }

        private void CheckBoxListRepeat_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBoxTrackRepeat.IsChecked = false;
            _isListRepeated = true;
            _isTrackRepeated = false;
        }

        private void CheckBoxTrackRepeat_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBoxListRepeat.IsChecked = false;
            _isTrackRepeated = true;
            _isListRepeated = false;
        }

        private void CheckBoxListAndTrackRepeat_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _isTrackRepeated = _isListRepeated = false;
        }
    }
}