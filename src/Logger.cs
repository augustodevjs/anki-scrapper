namespace scraping_anki
{
    public static class Logger
    {
        private static string LogFilePath => Path.Combine(Directory.GetCurrentDirectory(), "logs", "ankiscraper_app.log");

        public static void LogToFile(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now}] {message}";
                Console.WriteLine(logEntry);
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                try
                {
                    var fallbackLog = Path.Combine(Directory.GetCurrentDirectory(), "fallback_log.txt");
                    File.AppendAllText(fallbackLog, $"[{DateTime.Now}] Error writing to log: {ex.Message}" + Environment.NewLine);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}