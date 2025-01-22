using System.Text.RegularExpressions;

namespace JrEncoder;

public class Util
{
    public static HttpClient HttpClient { get; set; } = new(new SocketsHttpHandler()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
    });

    public static string GetExeLocation()
    {
        return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
    }

    /// <summary>
    /// Word wrap text
    /// </summary>
    /// <remarks>https://gist.github.com/anderssonjohan/660952</remarks>
    /// <param name="text"></param>
    /// <param name="maxLineLength"></param>
    /// <returns></returns>
    public static List<string> WordWrap(string text, int maxLineLength)
    {
        var list = new List<string>();

        int currentIndex;
        var lastWrap = 0;
        var whitespace = new[] { ' ', '\r', '\n', '\t' };
        do
        {
            currentIndex = lastWrap + maxLineLength > text.Length ? text.Length : (text.LastIndexOfAny(new[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' }, Math.Min(text.Length - 1, lastWrap + maxLineLength)) + 1);
            if (currentIndex <= lastWrap)
                currentIndex = Math.Min(lastWrap + maxLineLength, text.Length);
            list.Add(text.Substring(lastWrap, currentIndex - lastWrap).Trim(whitespace));
            lastWrap = currentIndex;
        } while (currentIndex < text.Length);

        return list;
    }

    public static string RemoveUnits(string text)
    {
        string pattern = @"(-?\d{1,3})F";
        string substitution = @"$1";
        RegexOptions options = RegexOptions.Multiline;

        Regex regex = new Regex(pattern, options);
        return regex.Replace(text, substitution);
    }
}