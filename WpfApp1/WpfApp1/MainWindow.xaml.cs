using System;
using System.IO;
using System.Media; // 引入 System.Media 命名空間
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer; // DispatcherTimer 實現倒數邏輯
        private TimeSpan _remainingTime; // 剩餘時間
        private bool _isPaused = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            MoveWindowToBottomRight();
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 關閉視窗
        }

        private void MoveWindowToBottomRight()
        {
            // 獲取主螢幕的寬度和高度
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // 將視窗定位到右下角，並留 10 像素邊距
            this.Left = screenWidth - this.Width - 10;
            this.Top = screenHeight - this.Height - 10;
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); // 啟用視窗拖曳
        }

        // 初始化 Timer
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        // 設定時間 (5、10、20 分鐘)
        private void SetTime_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && int.TryParse(button.Tag.ToString(), out int minutes))
            {
                _remainingTime = TimeSpan.FromMinutes(minutes);
                UpdateTimerDisplay();
            }
        }

        // 開始計時
        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (_remainingTime > TimeSpan.Zero)
            {
                _timer.Start();
                _isPaused = false;
            }
        }

        // 暫停計時
        private void PauseTimer_Click(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                _isPaused = true;
            }
        }

        // 重置計時
        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _remainingTime = TimeSpan.Zero;
            _isPaused = false;
            UpdateTimerDisplay();
        }

        // 每秒更新時間
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_remainingTime > TimeSpan.Zero)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerDisplay();
            }
            else
            {
                _timer.Stop();
                PlayCustomSound();
            }
        }

        private void PlayCustomSound()
        {
            // 初始化 MediaPlayer
            MediaPlayer mediaPlayer = new MediaPlayer();

            // 取得音檔路徑 (專案中的 Music 資料夾內的 500audio.mp3)
            string audioFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music", "500audio.mp3");

            // 檢查檔案是否存在
            if (File.Exists(audioFilePath))
            {
                // MessageBox.Show("音檔存在，路徑：" + audioFilePath); // 測試用，顯示檔案路徑

                mediaPlayer.Open(new Uri(audioFilePath, UriKind.Absolute));
                mediaPlayer.MediaEnded += (s, e) => mediaPlayer.Close(); // 避免重複播放時占用資源
                mediaPlayer.Play();
            }
            else
            {
                MessageBox.Show("音效檔案不存在！路徑：" + audioFilePath, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 更新時間顯示 (MM:SS)
        private void UpdateTimerDisplay()
        {
            TimerDisplay.Text = _remainingTime.ToString(@"mm\:ss");
        }
    }
}