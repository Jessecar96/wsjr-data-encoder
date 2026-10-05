using System.Xml.Serialization;
using JrEncoder.Schema;
using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.StarAttributes;

namespace JrEncoder;

public class StaticPageDownloader
{
    private Config _config;
    private DataTransmitter _dataTransmitter;
    private OMCW _omcw;
    private string _pagesDirectory;
    
    /// <summary>
    /// Start adding pages at this number.  See PageNum.cs for page number layout
    /// </summary>
    private static int START_STATIC_PAGE = 30;

    /// <summary>
    /// Max number of static pages that can fit in memory.  See PageNum.cs for page number layout
    /// </summary>
    private static int MAX_STATIC_PAGES = 18;

    /// <summary>
    /// Current page number we will use, start this at START_STATIC_PAGE
    /// </summary>
    private int _currentPageNumber = START_STATIC_PAGE;

    /// <summary>
    /// Map of page name => page number
    /// </summary>
    public static Dictionary<string, int> PageList = new Dictionary<string, int>();

    public StaticPageDownloader(Config config, DataTransmitter dataTransmitter, OMCW omcw)
    {
        _config = config;
        _dataTransmitter = dataTransmitter;
        _omcw = omcw;
        _pagesDirectory = Path.Combine(Util.GetExeLocation(), "Pages");
    }

    private void OnFileModified(object source, FileSystemEventArgs e)
    {
        Console.WriteLine($"[StaticPageDownloader] Modified: {e.FullPath}");
        UpdateSingle(Path.GetFileName(e.FullPath));
    }

    public void UpdateAll()
    {
        Logger.Info("[StaticPageDownloader] Updating all static pages...");
        foreach (string filePath in Directory.EnumerateFiles(_pagesDirectory, "*.xml"))
        {
            // Update the page, only pass in the file name
            UpdateSingle(Path.GetFileName(filePath));
        }
    }

    /// <summary>
    /// Update a single static page
    /// </summary>
    /// <param name="fileName">File name in the Pages directory</param>
    private void UpdateSingle(string fileName)
    {
        // Ensure we can even add any more pages
        if (_currentPageNumber >= START_STATIC_PAGE + MAX_STATIC_PAGES)
        {
            Logger.Error($"[StaticPageDownloader] No more than {MAX_STATIC_PAGES} pages can be added. Skipping {fileName}");
            return;
        }

        // Ensure the file exists
        string file = Path.Combine(_pagesDirectory, fileName);
        if (!File.Exists(file))
        {
            Logger.Error($"[StaticPageDownloader] File not found: {file}");
            return;
        }

        // Parse the file
        CustomPage? page;
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CustomPage));
            using StreamReader reader = new StreamReader(file);
            page = (CustomPage?)serializer.Deserialize(reader);
        }
        catch (Exception e)
        {
            Logger.Error($"[StaticPageDownloader] Error parsing XML file: {fileName} - {e}");
            return;
        }

        // Ensure the page parsed fully
        if (page == null)
        {
            Logger.Error($"[StaticPageDownloader] XML parsed to null: {fileName}");
            return;
        }

        // Find the requested page name
        string pageName = Path.GetFileNameWithoutExtension(fileName);

        // Ensure a name for the page is given
        if (string.IsNullOrEmpty(pageName))
        {
            Logger.Error($"[StaticPageDownloader] Invalid page name: {fileName}");
            return;
        }

        // Store that page name if we don't already have it
        if (!PageList.ContainsKey(pageName))
        {
            PageList[pageName] = _currentPageNumber;
            // Add one to currentPageNumber so the next one can use it
            _currentPageNumber++;
        }

        // Build the page to send to the stars
        PageBuilder staticPageBuilder = new PageBuilder(PageList[pageName], Address.All, _omcw);
        foreach (PageLine line in page.Line)
        {
            // Build TextLineAttributes from PageLine
            TextLineAttributes attributes = new TextLineAttributes
            {
                Separator = line.Separator,
                Flash = line.Flash,
                Reverse = line.Reverse,
                Border = line.Border,
                Color = line.Color,
                Width = line.Width,
                Height = line.Height
            };
            try
            {
                staticPageBuilder.AddLine(line.Text ?? "", attributes);
            }
            catch (Exception e)
            {
                // Catch any errors adding a line
                Logger.Error("[StaticPageDownloader] " + e);
            }
        }

        // Transmit that page
        _dataTransmitter.AddFrame(staticPageBuilder.Build());
        Logger.Info($"[StaticPageDownloader] Updated static page {pageName} as page number {PageList[pageName]}");
    }

    public void Run()
    {
        // Set up a filesystem watcher to check for changes to page files
        using FileSystemWatcher watcher = new FileSystemWatcher(_pagesDirectory);
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        watcher.Filter = "*.xml";
        watcher.IncludeSubdirectories = false;

        watcher.Changed += OnFileModified;
        watcher.Created += OnFileModified;
        watcher.Renamed += OnFileModified;
        watcher.Error += (s, e) => Console.WriteLine($"[StaticPageDownloader] Error: {e.GetException().Message}");

        // Start watching
        watcher.EnableRaisingEvents = true;
        Logger.Info("[StaticPageDownloader] Watching for page changes in " + _pagesDirectory);

        // Forever...
        while (true)
            Thread.Sleep(1000);
    }
}