namespace scraping_anki;

public static class FileUtils
{
    public static bool HasFileContentChanged(string? filePath, string lastContentHashFile)
    {
        if (!File.Exists(filePath))
        {
            Logger.LogToFile($"File {filePath} not found.");
            return false;
        }
        var currentHash = CalculateFileHash(filePath);

        if (!File.Exists(lastContentHashFile))
        {
            Logger.LogToFile("No previous content hash found. This is the first run.");
            return true;
        }
        var lastHash = File.ReadAllText(lastContentHashFile);
        var hasChanged = currentHash != lastHash;

        Logger.LogToFile(hasChanged 
            ? "Content has changed since last run. Will add cards." 
            : "Content has not changed since last run.");

        return hasChanged;
    }

    public static void SaveContentHash(string? filePath, string lastContentHashFile)
    {
        var hash = CalculateFileHash(filePath);
        File.WriteAllText(lastContentHashFile, hash);
    }

    private static string CalculateFileHash(string? filePath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }
}