using System.Text;

namespace LightningRod;

// https://www.infoworld.com/article/2238109/implement-a-simple-logger-in-csharp.html
public static class Logger
{
    static readonly object lockObj = new();
    public static string filePath = $"{GameData.DataPath}/LightningRod.log";
    public static StringBuilder logBuffer = new StringBuilder();

    public static void Log(string message)
    {
        logBuffer.AppendLine($"[{DateTime.Now:HH:mm:ss.ff}] {message}");
    }

    public static void MakeFile()
    {
        using (StreamWriter outputFile = new StreamWriter(filePath))
        {
            outputFile.WriteLine(logBuffer.ToString());
        }
    }
}
