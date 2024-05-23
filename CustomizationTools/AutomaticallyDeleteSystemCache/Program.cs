using Microsoft.VisualBasic.FileIO;

namespace AutomaticallyDeleteSystemCache
{
    public class Program
    {
        static void Main(string[] args)
        {
            string tempPath = @"C:\Users\kennyguo\AppData\Local\Temp";
            string logDirectory = @"D:\TempLog";
            string logFileName = $"{DateTime.Now:yyyyMMdd}_DeleteSysLogs.txt";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            try
            {
                // 確保日誌目錄存在
                Directory.CreateDirectory(logDirectory);
                using StreamWriter logWriter = new StreamWriter(logFilePath, true);

                logWriter.WriteLine($"[{DateTime.Now}] 開始刪除暫存資料。");
                DeleteDirectoryContents(tempPath, logWriter);
                logWriter.WriteLine($"[{DateTime.Now}] 暫存資料已成功刪除。");
                Console.WriteLine("暫存資料已成功刪除。");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除過程中發生錯誤: {ex.Message}");
            }
        }

        static void DeleteDirectoryContents(string directoryPath, StreamWriter logWriter)
        {
            #region 刪除目錄中的所有檔案
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                try
                {
                    FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    string logMessage = $"[{DateTime.Now}] 檔案已刪除: {file}";
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
                try
                {
                    FileSystem.DeleteDirectory(subdirectory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    string logMessage = $"[{DateTime.Now}] 目錄已刪除: {subdirectory}";
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
    }
}