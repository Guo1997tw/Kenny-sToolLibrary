using SharpVectors.Converters;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace prjUniversalTimer
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private int totalSeconds;
        private bool isRunning = false;
        private int lastHour;
        private int lastMinute;
        private int lastSecond;
        private bool isTimerRunning; // 用於控制滑桿的啟用/禁用
        private bool isWindowLocked = false;

        public MainWindow()
        {
            InitializeComponent();
            WindowFixed();
            InitializeTimer();
        }

        /// <summary>
        /// 視窗固定到螢幕右下角
        /// </summary>
        private void WindowFixed()
        {
            var width = SystemParameters.PrimaryScreenWidth;
            var height = SystemParameters.PrimaryScreenHeight;
            this.Left = width - this.Width - 10;
            this.Top = height - this.Height - 10;
        }

        /// <summary>
        /// 關閉視窗
        /// </summary>
        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 初始化計時器
        /// </summary>
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 每秒觸發一次
            timer.Tick += Timer_Tick;

            GetSliderValue(null, null);
        }

        /// <summary>
        /// 更新滑桿的啟用/禁用狀態
        /// </summary>
        private void UpdateSliderState()
        {
            // 禁用或啟用所有滑桿
            HourSlider.IsEnabled = !isTimerRunning;
            MinuteSlider.IsEnabled = !isTimerRunning;
            SecondSlider.IsEnabled = !isTimerRunning;
        }

        #region 計時器事件
        private void Timer_Tick(object? sender, EventArgs? e)
        {
            if (totalSeconds > 0)
            {
                totalSeconds--;
                UpdateTimeDisplay();
            }
            else
            {
                timer.Stop();
                isRunning = false;
                isTimerRunning = false; // 停止後啟用滑桿
                UpdateSliderState();

                HourSlider.Value = lastHour;
                MinuteSlider.Value = lastMinute;
                SecondSlider.Value = lastSecond;

                TimeDisplay.Text = $"{lastHour:00} : {lastMinute:00} : {lastSecond:00}";

                PlayNotificationSound();
            }
        }

        private void PlayNotificationSound()
        {
            try
            {
                string mp3Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Musics", "Audiobook.mp3");
                if (!System.IO.File.Exists(mp3Path))
                {
                    MessageBox.Show("MP3 文件不存在，請檢查路徑！", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var player = new System.Windows.Media.MediaPlayer();
                player.MediaEnded += (s, e) => player.Close();
                player.Open(new Uri(mp3Path, UriKind.Absolute));
                player.Volume = 1.0;
                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放提示音時發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 按鈕功能
        private void StartTimer(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;
                isTimerRunning = true; // 開始計時時禁用滑桿
                UpdateSliderState();

                if (totalSeconds == 0)
                {
                    lastHour = (int)HourSlider.Value;
                    lastMinute = (int)MinuteSlider.Value;
                    lastSecond = (int)SecondSlider.Value;

                    totalSeconds = lastHour * 3600 + lastMinute * 60 + lastSecond;
                }

                timer.Start();
            }
        }

        private void PauseTimer(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
                isTimerRunning = false; // 暫停後啟用滑桿
                UpdateSliderState();
            }
        }

        private void ResetTimer(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            isRunning = false;
            isTimerRunning = false; // 重置後啟用滑桿
            UpdateSliderState();

            lastHour = 0;
            lastMinute = 0;
            lastSecond = 0;

            HourSlider.Value = 0;
            MinuteSlider.Value = 0;
            SecondSlider.Value = 0;

            totalSeconds = 0;

            UpdateTimeDisplay();
        }

        /// <summary>
        /// 鎖定視窗
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleWindowLock(object sender, RoutedEventArgs e)
        {
            // 切換鎖定狀態
            isWindowLocked = !isWindowLocked;

            // Debug 確認鎖定狀態變化
            Debug.WriteLine($"Window Lock State: {isWindowLocked}");

            // 根據狀態切換按鈕顯示
            if (isWindowLocked)
            {
                LockButton.Visibility = Visibility.Collapsed;
                UnLockButton.Visibility = Visibility.Visible;
            }
            else
            {
                LockButton.Visibility = Visibility.Visible;
                UnLockButton.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 滑桿同步顯示
        private void GetSliderValue(object? sender, RoutedPropertyChangedEventArgs<double>? e)
        {
            if (isTimerRunning)
                return; // 計時期間不處理滑桿變更

            HourValue.Text = ((int)HourSlider.Value).ToString();
            MinuteValue.Text = ((int)MinuteSlider.Value).ToString();
            SecondValue.Text = ((int)SecondSlider.Value).ToString();

            lastHour = (int)HourSlider.Value;
            lastMinute = (int)MinuteSlider.Value;
            lastSecond = (int)SecondSlider.Value;

            totalSeconds = lastHour * 3600 + lastMinute * 60 + lastSecond;
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            TimeDisplay.Text = $"{hours:00} : {minutes:00} : {seconds:00}";
        }
        #endregion

        #region 視窗拖曳移動
        private void WindowMovement(object sender, MouseButtonEventArgs e)
        {
            if (isWindowLocked)
            {
                // 當視窗鎖定時，禁止拖動
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        #endregion
    }
}