using JrEncoderLib;
using JrEncoderLib.DataTransmitter;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

namespace JrEncoder;

/// <summary>
/// Flavor manager, obviously
/// </summary>
public class FlavorMan(Config config, Flavors flavors, DataTransmitter dataTransmitter, OMCW omcw)
{
    private CancellationTokenSource? _cancellationTokenSource = null;
    private bool _flavorRunning;
    private bool _runLoop = false;

    public void ShowUpdatePage()
    {
        // Updating page
        DataFrame[] updatePage = new PageBuilder(41, Address.All, omcw)
            .AddLine("                                ")
            .AddLine("                                ")
            .AddLine("                                ")
            .AddLine("           Please Wait          ")
            .AddLine("  Information is being updated  ")
            .Build();
        dataTransmitter.AddFrame(updatePage);

        // Show updating page
        omcw
            .BottomSolid()
            .TopSolid()
            .TopPage(41)
            .RegionSeparator()
            .LDL(LDLStyle.DateTime)
            .Commit();
    }

    /// <summary>
    /// Restore to the default OMCW state
    /// </summary>
    public void SetDefaultOMCW()
    {
        // Get default flavor
        Flavor? flavor = flavors.Flavor.FirstOrDefault(el => el.Name == "Default");
        if (flavor == null)
        {
            Logger.Error("Default flavor does not exist. Using default OMCW.");
            omcw.TopSolid(false)
                .TopPage(0)
                .BottomSolid(false)
                .RegionSeparator(false)
                .LDL(LDLStyle.DateTime)
                .Commit();
        }
        else
        {
            // Default flavor only supports one page
            FlavorPage page = flavor.Page[0];

            // Parse the page number
            if (!Enum.TryParse(page.Name, out Page newPage))
            {
                Logger.Error($"Invalid page \"{page.Name}\"");
                Program.ShowErrorMessage($"Invalid page \"{page.Name}\"");
                return;
            }

            // Parse LDL style
            // Make sure that LDL style exists
            if (!Enum.TryParse(page.LDL, out LDLStyle ldlStyle))
            {
                Logger.Error($"Invalid LDL Style \"{page.LDL}\"");
                Program.ShowErrorMessage($"Invalid LDL Style \"{page.LDL}\"");
                return;
            }

            // Set omcw properties
            omcw
                .TopPage((int)newPage)
                .LDL(ldlStyle)
                .TopSolid(page.TopSolid)
                .BottomSolid(page.BottomSolid)
                .RegionSeparator(page.RegionSeparator)
                .Radar(page.Radar)
                .AuxAudio(page.AuxAudio)
                .LocalPreroll(page.LocalPreroll)
                .LocalProgram(page.LocalProgram)
                .Commit();

            Logger.Info("Switched to default page");
        }
    }

    /// <summary>
    /// Run an LF flavor, optionally at a specific time
    /// </summary>
    /// <param name="flavorName">Name of flavor, defined in Flavors.xml</param>
    /// <param name="runTime">Specific time to run the flavor at</param>
    public async Task RunFlavor(string flavorName, DateTimeOffset? runTime = null)
    {
        // Make a new CTS if we don't have one, or the one we have was already canceled previously
        if (_cancellationTokenSource is null or { IsCancellationRequested: true })
            _cancellationTokenSource = new CancellationTokenSource();

        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        if (_flavorRunning)
        {
            Logger.Error("Flavor is already running. Not running another!");
            return;
        }

        // Find the flavor defined in Flavors.xml
        Flavor? flavor = flavors.Flavor.FirstOrDefault(el => el.Name == flavorName);

        // Could not find that flavor
        if (flavor == null)
        {
            Logger.Error($"Flavor \"{flavorName}\" does not exist in Flavors.xml");
            Program.ShowErrorMessage($"Flavor \"{flavorName}\" does not exist in Flavors.xml");
            return;
        }

        if (runTime != null)
        {
            long currentTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long runTimeUnix = runTime.Value.ToUnixTimeSeconds();

            // Make sure schedule time was not in the past
            if (runTime.Value <= DateTime.Now)
            {
                Logger.Error($"Flavor \"{flavorName}\" was scheduled to run before current time. Current time: {currentTimeUnix} scheduled: {runTimeUnix}");
                return;
            }

            // Find the difference between now and run time
            long secondsDifference = runTimeUnix - currentTimeUnix;
            Logger.Info($"Flavor \"{flavorName}\" will run in {secondsDifference} seconds");
            _flavorRunning = true;

            try
            {
                // Wait until the time we want
                await Task.Delay(TimeSpan.FromSeconds(secondsDifference), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // We were signaled to stop so stop
                Logger.Info("RunFlavor Cancelled");
                return;
            }
        }

        Logger.Info($"Running flavor \"{flavor.Name}\"");
        _flavorRunning = true;

        // TODO: Save OMCW state to restore to after

        foreach (FlavorPage page in flavor.Page)
        {
            // Make sure that page exists
            if (!Enum.TryParse(page.Name, out Page newPage))
            {
                Logger.Error($"Invalid page \"{page.Name}\"");
                Program.ShowErrorMessage($"Invalid page \"{page.Name}\"");
                return;
            }

            // Check if this page is changing LDL style
            if (!string.IsNullOrEmpty(page.LDL))
            {
                // Make sure that LDL style exists
                if (!Enum.TryParse(page.LDL, out LDLStyle ldlStyle))
                {
                    Logger.Error($"Invalid LDL Style \"{page.LDL}\"");
                    Program.ShowErrorMessage($"Invalid LDL Style \"{page.LDL}\"");
                    return;
                }

                // Set that LDL style
                omcw.LDL(ldlStyle);
            }

            // Switch to the page, set all other OMCW attributes
            omcw
                .TopPage((int)newPage)
                .TopSolid(page.TopSolid)
                .BottomSolid(page.BottomSolid)
                .RegionSeparator(page.RegionSeparator)
                .Radar(page.Radar)
                .AuxAudio(page.AuxAudio)
                .LocalPreroll(page.LocalPreroll)
                .LocalProgram(page.LocalProgram)
                .Commit();

            Logger.Info("Switched to page " + newPage);

            // Wait for its duration
            try
            {
                await Task.Delay(page.Duration * 1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // We were signaled to stop so stop
                Logger.Info("RunFlavor Cancelled");
                return;
            }
        }

        // we done!
        _flavorRunning = false;
        Logger.Info($"Flavor \"{flavor.Name}\" complete");

        // Switch to default state if it's not looping
        if (!_runLoop)
            SetDefaultOMCW();
    }

    public void CancelLF()
    {
        Logger.Info("Canceling LF");

        // Stop any looping from looping again
        _runLoop = false;

        // This will cancel any Task.Delay's in RunFlavor
        _cancellationTokenSource?.Cancel();

        // No longer running an LF
        _flavorRunning = false;

        // Switch to default state
        SetDefaultOMCW();
    }

    public async Task RunLoop(string flavorName)
    {
        // Make sure we don't run on top of anything else
        if (_flavorRunning || _runLoop)
        {
            Logger.Error("Flavor is already running. Not running another!");
            return;
        }

        // We want to start looping. This is changed in CancelLF() to stop it 
        _runLoop = true;

        // Loop that flavor forever
        while (_runLoop)
            await RunFlavor(flavorName);
    }
}