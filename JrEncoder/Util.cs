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

    public static List<string> WordWrapAlert(string text, int maxLineLength = 32)
    {
        // Fix windows (UGH) line endings
        text = text.Replace("\r\n", "\n");

        // Split text into paragraphs
        string[] paragraphs = text.Split(new string[] { "\n\n" }, StringSplitOptions.None);

        // 2D list, paragraph[lines in paragraph]
        List<List<string>> parahraphsList = new();

        for (int i = 0; i < paragraphs.Length; i++)
        {
            // Remove line breaks in the paragraph
            paragraphs[i] = paragraphs[i].Replace("\n", " ");

            // Remove any multi-spaces caused by NWS indenting the text
            paragraphs[i] = Regex.Replace(paragraphs[i], @"\s+", " ");

            // Word wrap to line length
            parahraphsList.Add(WordWrap(paragraphs[i], maxLineLength));
        }

        // Now join the lists with blank entries in between them
        List<string> output = new();
        for (int i = 0; i < parahraphsList.Count; i++)
        {
            output.AddRange(parahraphsList[i]);
            // If this is not the last element, add a blank line between the paragraphs
            if (i != parahraphsList.Count - 1)
                output.Add("");
        }

        return output;
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

    /// <summary>
    /// Center a string by adding padding within the specified with
    /// </summary>
    /// <param name="s"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public static string CenterString(string s, int width = 32)
    {
        if (s.Length >= width)
        {
            return s;
        }

        int leftPadding = (width - s.Length) / 2;
        int rightPadding = width - s.Length - leftPadding;

        return new string(' ', leftPadding) + s + new string(' ', rightPadding);
    }

    /// <summary>
    /// Format wind and speed for use on "Latest Hourly Observations"
    /// </summary>
    /// <param name="windDir"></param>
    /// <param name="windSpeed"></param>
    /// <returns></returns>
    public static string FormatWindAndSpeed(string windDir, string windSpeed)
    {
        if (int.Parse(windSpeed) == 0) return "Calm";

        if (windDir.Length == 3 && windSpeed.Length == 1)
            return windDir + windSpeed;
        if (windDir.Length == 3 && windSpeed.Length == 2)
            // Remove first char from direction to fit
            return windDir.Remove(0, 1) + windSpeed;
        if (windDir.Length == 3 && windSpeed.Length == 3)
            // Remove first 2 chars from direction to fit
            return windDir.Remove(0, 2) + windSpeed;

        if (windDir.Length == 2 && windSpeed.Length == 1)
            return windDir + " " + windSpeed;
        if (windDir.Length == 2 && windSpeed.Length == 2)
            return windDir + windSpeed;
        if (windDir.Length == 2 && windSpeed.Length == 3)
            // Remove first char from direction to fit
            return windDir.Remove(0, 1) + windSpeed;

        if (windDir.Length == 1 && windSpeed.Length == 1)
            return windDir + "  " + windSpeed;
        if (windDir.Length == 1 && windSpeed.Length == 2)
            return windDir + " " + windSpeed;
        if (windDir.Length == 1 && windSpeed.Length == 3)
            return windDir + windSpeed;

        return windSpeed;
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