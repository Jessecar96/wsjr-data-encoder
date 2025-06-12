namespace JrEncoder;

public class Logger
{
    private static void Log(string msg)
    {
        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + msg);
    }
    
    public static void Debug(string message)
    {
        Log("DEBUG " + message);
    }
    
    public static void Info(string message)
    {
        Log("INFO " + message);
    }
    
    public static void Error(string message)
    {
        Log("ERROR " + message);
    }
}