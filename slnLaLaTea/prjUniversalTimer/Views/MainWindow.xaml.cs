using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace prjUniversalTimer
{
    public partial class MainWindow : Window
    {
        // 計時器
        private DispatcherTimer timer;
        // 總秒數
        private int totalSeconds;
        // 計時器狀態
        private bool isRunning = false;
        private int lastHour;
        private int lastMinute;
        private int lastSecond;

        public MainWindow()
        {
            InitializeComponent();
            WindowFixed();
            InitializeTimer();
        }

        /// <summary>
        /// 視窗固定
        /// </summary>
        private void WindowFixed()
        {
            var width = SystemParameters.PrimaryScreenWidth;
            var height = SystemParameters.PrimaryScreenHeight;

            // 固定右下角 (x, y軸)
            this.Left = width - this.Width - 10;
            this.Top = height - this.Height - 10;
        }

        /// <summary>
        /// 關閉視窗
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            // 初始化滑桿數值與顯示
            GetSliderValue(null, null);
        }

        #region 計時器事件
        /// <summary>
        /// 每秒觸發，倒數計時
        /// </summary>
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

                // 時間到時，將滑桿和時間顯示恢復為最後設定值
                HourSlider.Value = lastHour;
                MinuteSlider.Value = lastMinute;
                SecondSlider.Value = lastSecond;

                TimeDisplay.Text = $"{lastHour:00} : {lastMinute:00} : {lastSecond:00}";

                PlayNotificationSound();
            }
        }

        /// <summary>
        /// 播放音檔
        /// </summary>
        private void PlayNotificationSound()
        {
            try
            {
                // 確保 MP3 文件路徑正確
                string mp3Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Musics", "Audiobook.mp3");
                Debug.WriteLine($"MP3 路徑: {mp3Path}");

                if (!System.IO.File.Exists(mp3Path))
                {
                    MessageBox.Show("MP3 文件不存在，請檢查路徑！", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 建立 MediaPlayer 實例
                var player = new System.Windows.Media.MediaPlayer();

                // 設置播放完成後釋放資源
                player.MediaEnded += (s, e) =>
                {
                    player.Close();
                    // Debug.WriteLine("MediaPlayer 資源已釋放");
                };

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
        /// <summary>
        /// 開始按鈕事件
        /// </summary>
        private void StartTimer(object sender, RoutedEventArgs e)
        {
            // 如果計時器未運行，則啟動
            if (!isRunning)
            {
                isRunning = true;

                // 只有 totalSeconds 為 0 (初始狀態) 時才重新計算
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

        /// <summary>
        /// 暫停按鈕事件
        /// </summary>
        private void PauseTimer(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
            }
        }

        /// <summary>
        /// 重置按鈕事件
        /// </summary>
        private void ResetTimer(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            isRunning = false;

            // 清除儲存的初始值
            lastHour = 0;
            lastMinute = 0;
            lastSecond = 0;

            // 重置滑桿和時間顯示
            HourSlider.Value = 0;
            MinuteSlider.Value = 0;
            SecondSlider.Value = 0;

            totalSeconds = 0;

            UpdateTimeDisplay();
        }
        #endregion

        #region 滑桿同步顯示
        /// <summary>
        /// 滑桿數值變化事件
        /// </summary>
        private void GetSliderValue(object? sender, RoutedPropertyChangedEventArgs<double>? e)
        {
            // 更新滑桿旁的數值
            HourValue.Text = ((int)HourSlider.Value).ToString();
            MinuteValue.Text = ((int)MinuteSlider.Value).ToString();
            SecondValue.Text = ((int)SecondSlider.Value).ToString();

            // 儲存目前滑桿的最新設定值
            lastHour = (int)HourSlider.Value;
            lastMinute = (int)MinuteSlider.Value;
            lastSecond = (int)SecondSlider.Value;

            // 同步更新時間顯示
            totalSeconds = lastHour * 3600 + lastMinute * 60 + lastSecond;
            UpdateTimeDisplay();
        }

        /// <summary>
        /// 更新時間顯示區塊
        /// </summary>
        private void UpdateTimeDisplay()
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            // 更新時間顯示的 TextBlock
            TimeDisplay.Text = $"{hours:00} : {minutes:00} : {seconds:00}";
        }
        #endregion

        #region 視窗拖曳移動
        private void WindowMovement(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        #endregion
    }
}