using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
        private readonly string _validSerial = "12345-ABCDE-67890"; // 合法啟動序號
        private readonly string LicenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            MoveWindowToBottomRight();
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;

            // 檢查授權狀態
            if (IsLicenseValid())
            {
                StartupGrid.Visibility = Visibility.Collapsed;
                TimerGrid.Visibility = Visibility.Visible;
            }
            else
            {
                StartupGrid.Visibility = Visibility.Visible;
                TimerGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 關閉視窗
        }

        private void MoveWindowToBottomRight()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Left = screenWidth - this.Width - 10;
            this.Top = screenHeight - this.Height - 10;
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); // 啟用視窗拖曳
        }

        // 啟動序號驗證按鈕
        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            if (SerialInput.Password == _validSerial)
            {
                var machineInfo = GetMachineIdentifier();
                SaveLicense(_validSerial, machineInfo);

                StartupGrid.Visibility = Visibility.Collapsed;
                TimerGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("序號無效！請重新輸入。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int minutes))
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
            MediaPlayer mediaPlayer = new MediaPlayer();
            string audioFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music", "500audio.mp3");

            if (File.Exists(audioFilePath))
            {
                mediaPlayer.Open(new Uri(audioFilePath, UriKind.Absolute));
                mediaPlayer.MediaEnded += (s, e) => mediaPlayer.Close();
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

        // 獲取主機唯一識別碼
        private string GetMachineIdentifier()
        {
            var macAddress = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();

            return macAddress ?? Environment.MachineName;
        }

        // 儲存授權資訊
        private void SaveLicense(string serial, string machineInfo)
        {
            var data = $"{serial}|{machineInfo}";
            var encryptedData = Encrypt(data, "MySecretKey12345");
            File.WriteAllText(LicenseFilePath, encryptedData);
        }

        // 加密資料
        private string Encrypt(string plainText, string key)
        {
            using (var aes = Aes.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(16).Substring(0, 16));
                aes.Key = keyBytes;
                aes.IV = new byte[16];
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // 驗證授權
        private bool IsLicenseValid()
        {
            if (File.Exists(LicenseFilePath))
            {
                try
                {
                    var encryptedData = File.ReadAllText(LicenseFilePath);
                    var decryptedData = Decrypt(encryptedData, "MySecretKey12345");
                    var parts = decryptedData.Split('|');
                    if (parts.Length == 2)
                    {
                        var serial = parts[0];
                        var machineInfo = parts[1];
                        return serial == _validSerial && machineInfo == GetMachineIdentifier();
                    }
                }
                catch
                {
                    // 解密失敗
                    return false;
                }
            }
            return false;
        }

        // 解密資料
        private string Decrypt(string encryptedText, string key)
        {
            using (var aes = Aes.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(16).Substring(0, 16));
                aes.Key = keyBytes;
                aes.IV = new byte[16];
                using (var ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}