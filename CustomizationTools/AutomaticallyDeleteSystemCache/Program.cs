using Microsoft.VisualBasic.FileIO;
using System.Runtime.InteropServices;

namespace AutomaticallyDeleteSystemCache
{
    public class Program
    {
        #region API函數
        // 定義用於查找和點擊窗口的Windows API函數
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;

        // 用於控制線程的標誌變數
        private static bool _shouldStop = false;
        #endregion

        #region 主程式
        static async Task Main(string[] args)
        {
            string userName = Environment.UserName;
            string tempPath = $@"C:\Users\{userName}\AppData\Local\Temp";
            string logDirectory = @"D:\TempLog";
            string logFileName = $"{DateTime.Now:yyyyMMdd}_DeleteSysLogs.txt";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            try
            {
                // 確保日誌目錄存在
                Directory.CreateDirectory(logDirectory);
                using StreamWriter logWriter = new StreamWriter(logFilePath, true);

                logWriter.WriteLine($"[{DateTime.Now}] 開始刪除暫存資料。");

                // 啟動一個新線程來監控並關閉彈出對話框
                var cts = new CancellationTokenSource();
                var alertMonitorTask = Task.Run(() => MonitorAndCloseAlertDialogs(cts.Token), cts.Token);

                await DeleteDirectoryContents(tempPath, logWriter);
                logWriter.WriteLine($"[{DateTime.Now}] 暫存資料已成功刪除。");
                Console.WriteLine("暫存資料已成功刪除。");

                // 停止監控彈出對話框
                _shouldStop = true;
                cts.Cancel();
                await alertMonitorTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除過程中發生錯誤: {ex.Message}");
            }
        }
        #endregion

        #region 刪除物件方法
        static async Task DeleteDirectoryContents(string directoryPath, StreamWriter logWriter)
        {
            #region 刪除目錄中的所有檔案
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    await Task.Run(() =>
                    {
                        FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        string logMessage = $"[{DateTime.Now}] 檔案已刪除: {file}";
                        logWriter.WriteLine(logMessage);
                        Console.WriteLine(logMessage);
                    }, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    string logMessage = $"[{DateTime.Now}] 刪除檔案超時: {file}";
                    logWriter.WriteLine(logMessage);
                    Console.WriteLine(logMessage);
                }
                catch (Exception ex)
                {
                    string logMessage = $"[{DateTime.Now}] 無法刪除檔案: {file}, 錯誤: {ex.Message}";
                    logWriter.WriteLine(logMessage);
                    Console.WriteLine(logMessage);
                }
            }
            #endregion

            #region 刪除目錄中的所有子目錄
            foreach (var subdirectory in Directory.GetDirectories(directoryPath))
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    await Task.Run(() =>
                    {
                        FileSystem.DeleteDirectory(subdirectory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        string logMessage = $"[{DateTime.Now}] 目錄已刪除: {subdirectory}";
                        logWriter.WriteLine(logMessage);
                        Console.WriteLine(logMessage);
                    }, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    string logMessage = $"[{DateTime.Now}] 刪除目錄超時: {subdirectory}";
                    logWriter.WriteLine(logMessage);
                    Console.WriteLine(logMessage);
                }
                catch (Exception ex)
                {
                    string logMessage = $"[{DateTime.Now}] 無法刪除目錄: {subdirectory}, 錯誤: {ex.Message}";
                    logWriter.WriteLine(logMessage);
                    Console.WriteLine(logMessage);
                }
            }
            #endregion
        }
        #endregion

        #region 監控告警視窗方法
        static void MonitorAndCloseAlertDialogs(CancellationToken token)
        {
            while (!_shouldStop && !token.IsCancellationRequested)
            {
                // 這裡的標題應根據實際情況調整
                IntPtr alertWindow = FindWindow("#32770", "刪除檔案");

                if (alertWindow != IntPtr.Zero)
                {
                    // 同樣根據實際情況調整
                    IntPtr cancelButton = FindWindowEx(alertWindow, IntPtr.Zero, "Button", "取消");

                    if (cancelButton != IntPtr.Zero)
                    {
                        PostMessage(cancelButton, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
                        PostMessage(cancelButton, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
                    }
                }

                // 每秒檢查一次
                Thread.Sleep(1000);
            }
        }
        #endregion
    }
}