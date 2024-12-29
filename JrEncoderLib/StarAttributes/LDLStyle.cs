namespace JrEncoderLib.StarAttributes;

public enum LDLStyle
{
    /// <summary>
    /// Show nothing on the LDL
    /// </summary>
    Nothing = 0,
    
    /// <summary>
    /// Show the date, time, and local weather sensors if present. If not, show page 50
    /// </summary>
    DateTime = 1,
    
    /// <summary>
    /// Show the local ad crawl, entered via keyboard on the front of the unit
    /// </summary>
    LocalCrawl = 2,
    
    /// <summary>
    /// Show page 51 in a crawl, no date and time
    /// </summary>
    AlternateCrawl = 3
}