namespace JrEncoder;

public class Util
{
    public static HttpClient HttpClient { get; set; } = new(new SocketsHttpHandler()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
    });

    public static string GetExeLocation()
    {
        return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    }
}