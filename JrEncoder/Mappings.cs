using System.Text.Json;

namespace JrEncoder;

public class Mappings
{
    private static Dictionary<string, string>? _obsMapping = null;
    private static Dictionary<string, string>? _fcstMapping = null;

    public static string GetObsMapping(int code)
    {
        if (_obsMapping == null)
        {
            // Load obs mapping
            string path = Path.Combine(Util.GetExeLocation(), "ObsMapping.json");
            try
            {
                _obsMapping = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Mappings] Failed to load ObsMapping.json: " + ex.Message);
                return "N/A";
            }
        }

        if (_obsMapping == null)
            return "N/A";

        // Find obs mapping
        return _obsMapping.GetValueOrDefault(code.ToString(), "N/A");
    }

    public static string GetFcstMapping(int code)
    {
        if (_fcstMapping == null)
        {
            // Load fcst mapping
            string path = Path.Combine(Util.GetExeLocation(), "FcstMapping.json");
            try
            {
                _fcstMapping = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Mappings] Failed to load FcstMapping.json: " + ex.Message);
                return "N/A";
            }
        }

        if (_fcstMapping == null)
            return "N/A";

        // Find fcst mapping
        return _fcstMapping.GetValueOrDefault(code.ToString(), "N/A");
    }
}