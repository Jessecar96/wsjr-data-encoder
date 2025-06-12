using System.Text.Json;

namespace JrEncoder;

public class Mappings
{
    private static Dictionary<string, string>? _obsMapping = null;
    private static Dictionary<string, string>? _fcstMapping = null;
    private static Dictionary<string, List<string>>? _fcstTwoLineMapping = null;

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
                Logger.Error("[Mappings] Failed to load ObsMapping.json: " + ex.Message);
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
                Logger.Error("[Mappings] Failed to load FcstMapping.json: " + ex.Message);
                return "N/A";
            }
        }

        if (_fcstMapping == null)
            return "N/A";

        // Find fcst mapping
        return _fcstMapping.GetValueOrDefault(code.ToString(), "N/A");
    }

    public static List<string> GetFcstTwoLineMapping(int code)
    {
        if (_fcstTwoLineMapping == null)
        {
            // Load fcst mapping
            string path = Path.Combine(Util.GetExeLocation(), "FcstTwoLineMapping.json");
            try
            {
                _fcstTwoLineMapping = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Logger.Error("[Mappings] Failed to load FcstTwoLineMapping.json: " + ex.Message);
                return ["N/A", ""];
            }
        }

        if (_fcstTwoLineMapping == null)
            return ["N/A", ""];

        // Find fcst mapping
        return _fcstTwoLineMapping.GetValueOrDefault(code.ToString(), ["N/A", ""]);
    }
}