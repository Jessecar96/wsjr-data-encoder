using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.StarAttributes;

namespace JrEncoder;

class Program
{
    private static OMCW? _omcw;
    private static Flavors? _flavors;
    private static Config? _config;
    private static TimeUpdater? _timeUpdater;
    private static DataTransmitter? _dataTransmitter;
    private static WebServer? _webServer;
    public static DataDownloader? Downloader;
    public static FlavorMan? FlavorMan;

    private static async Task Main(string[] args)
    {
        Logger.Info("Starting WeatherSTAR Data Transmitter");
        Logger.Info("Written by Jesse Cardone, 2024");

        // Config creation parameter
        if (args.Contains("--create-config"))
        {
            if (File.Exists(Config.GetPath()))
            {
                Logger.Error("config.json already exists. Please delete it to create a new one.");
                return;
            }

            Config.CreateConfig();
            Logger.Info("Created config.json, program will now exit.");
            return;
        }

        // Setup http client
        Util.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JrEncoder/1.0");

        // Build our OMCW of defaults
        _omcw = OMCW.Create()
            .BottomSolid(false)
            .TopSolid(false)
            .TopPage(0)
            .RegionSeparator(false)
            .LDL(LDLStyle.DateTime)
            .Commit();

        if (args.Contains("--null-transmitter"))
        {
            // Use null transmitter, for debugging on a non-raspberry pi
            _dataTransmitter = new NullDataTransmitter(_omcw);
        }
        else
        {
            // Init GPIO data transmitter, sets up DDS module
            _dataTransmitter = new GPIODataTransmitter(_omcw);
            _dataTransmitter.Init();
        }

        // Background thread for data transmission
        _ = Task.Run(() =>
        {
            try
            {
                _dataTransmitter.Run();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to run data transmitter: " + ex.Message);
                if (ex.StackTrace != null) Logger.Error(ex.StackTrace);
            }
        });

        // Load config.json
        await LoadConfig("config.json");

        // Load flavor config
        _flavors = Flavors.LoadFlavors();

        if (_flavors == null)
        {
            // No flavors :((
            Logger.Error("Failed to load Flavors.xml");
            ShowErrorMessage("Failed to load Flavors.xml", true);
            return;
        }

        // Init time updater
        _timeUpdater = new TimeUpdater(_config, _dataTransmitter, _omcw);
        _timeUpdater.Run();

        // Init data downloader
        Downloader = new DataDownloader(_config, _dataTransmitter, _omcw);

        // Start web server
        _webServer = new WebServer(_config, _flavors, _omcw);
        _ = _webServer.Run().ContinueWith(_ => Environment.Exit(0));
        
        // Init flavor man
        FlavorMan = new FlavorMan(_config, _flavors, _dataTransmitter, _omcw);

        // Show "Information is being updated" page
        FlavorMan.ShowUpdatePage();

        // Test for internet access
        do
        {
            // Run HTTP request to test connectivity to weather.gov
            try
            {
                await Util.HttpClient.GetAsync("https://api.weather.gov/alerts/active?zone=GAZ045");
                break; // Request worked! Break out of the do/while loop
            }
            catch (Exception ex)
            {
                // Failed to connect. Show error screen and try again in 10 seconds
                ShowErrorMessage("Failed to connect to weather.gov. Retrying...");
                Logger.Error("Failed to connect to weather.gov: " + ex.Message);
                await Task.Delay(10000);
            }
        } while (true);

        // Update all records
        await Downloader.UpdateAll();

        // Background thread for data downloading
        try
        {
            Downloader.Run();
        }
        catch (Exception e)
        {
            ShowErrorMessage("Download failed: " + e.Message, true);
            Console.WriteLine(e);
        }

        await Task.Delay(500);

        // Check if looping is configured
        if (!string.IsNullOrEmpty(_config.LoopFlavor))
        {
            // await here if we're configured to run a loop by default
            // If aborted we will break out and go to the Forever... loop
            await FlavorMan.RunLoop(_config.LoopFlavor);
        }
        else
        {
            // Default no loop state
            FlavorMan.SetDefaultOMCW();
            Logger.Info("Ready for commands! Looping is not enabled.");
        }

        // Forever...
        while (true)
            Thread.Sleep(1000);
    }

    /// <summary>
    /// Load a config file
    /// </summary>
    public static async Task LoadConfig(string fileName)
    {
        try
        {
            _config = await Config.LoadConfig(fileName);
            if (Downloader != null)
                Downloader.SetConfig(_config);
            if (_timeUpdater != null)
                _timeUpdater.SetConfig(_config);
            if (_webServer != null)
                _webServer.SetConfig(_config);
            Logger.Info("Loaded config file " + fileName);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to load {fileName}: " + e.Message);
            ShowErrorMessage($"Failed to load {fileName}: " + e.Message, true);
        }
    }

    public static void ShowWxWarning(string message, WarningType type, Address address, OMCW omcw)
    {
        ShowWxWarning(message.Split('\n').ToList(), type, address, omcw);
    }

    public static void ShowWxWarning(List<string> message, WarningType type, Address address, OMCW omcw)
    {
        // Split that into chunks of 9 lines each
        string[][] chunks = message.Chunk(9).ToArray();

        int pageOffset = 0;
        for (int i = 0; i < chunks.Length; i++)
        {
            // Make a new page
            PageBuilder page = new PageBuilder((int)Page.WxWarning + pageOffset, address, omcw);

            // Set page attributes
            bool firstPage = i == 0;
            bool lastPage = i == chunks.Length - 1;
            bool multiPage = chunks.Length != 1;
            PageAttributes pageAttributes = new PageAttributes
            {
                Roll = true, // Every page needs to roll
                Chain = (multiPage && !lastPage) // Set chain when we have more than 1 page, and it's not the last page
            };

            // Set warning/advistory depending on type
            if (type == WarningType.Warning)
                pageAttributes.Warning = firstPage; // Only the first page in the chain gets the warning bit set
            else if (type == WarningType.Advisory)
                pageAttributes.Advisory = firstPage; // Only the first page in the chain gets the warning bit set

            page.Attributes(pageAttributes);

            // Add all the chunked lines
            foreach (string line in chunks[i])
                page.AddLine(line, new TextLineAttributes
                {
                    Color = (type == WarningType.Warning) ? Color.Red : Color.Brown
                });

            // Add to offset for the next loop
            pageOffset++;

            // Send it
            _dataTransmitter.AddFrame(page.Build());
            Logger.Info("Sent page " + page.PageNumber);
        }

        Logger.Info("Weather warning activated");
    }

    /// <summary>
    /// Show an error message page
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fatal">If this message should show forever, locking up the program</param>
    public static void ShowErrorMessage(string message, bool fatal = false)
    {
        PageBuilder page = new PageBuilder((int)Page.Error, Address.All, _omcw)
            .AddLine(Util.CenterString("ATTENTION CABLE OPERATOR"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine(Util.CenterString("An Error Has Occurred"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine(Util.CenterString("Restart Software Once Corrected"), new TextLineAttributes { Color = Color.Diarrhea })
            .AddLine("", new TextLineAttributes { Color = Color.Diarrhea, Height = 0 });

        // Word wrap message and add it
        List<string> messages = Util.WordWrap(message, 32);
        foreach (string s in messages)
            page.AddLine(Util.CenterString(s), new TextLineAttributes { Color = Color.Diarrhea });

        _dataTransmitter.AddFrame(page.Build());

        // Wait for the page to load into memory...
        Thread.Sleep(500);

        // Show blank page
        _omcw.TopSolid(true)
            .BottomSolid(true)
            .RegionSeparator(true)
            .TopPage(0)
            .Commit();

        // Wait for that to happen...
        Thread.Sleep(500);

        // Show error page
        _omcw.TopSolid(true)
            .BottomSolid(true)
            .RegionSeparator(true)
            .TopPage((int)Page.Error)
            .Commit();

        // Fatal error will cause the program to stop here forever
        if (fatal)
            while (true)
                Thread.Sleep(1000);
    }
}