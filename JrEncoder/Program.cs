using JrEncoder.Frames;
using JrEncoder.StarAttributes;

namespace JrEncoder;

class Program
{
    static void Main(string[] args)
    {
        // Build our OMCW
        OMCW omcw = OMCW.Create()
            .BottomSolid()
            .TopSolid()
            .TopPage(50)
            .RegionSeparator()
            .LDL(LDLStyle.DateTime)
            .Commit();

        // Init data transmitter, sets up DDS module
        DataTransmitter transmitter = new(omcw);
        transmitter.Init();

        // Background thread to keep data transmitter running
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            transmitter.Run();
        }).Start();
        
        // Send date & time
        TimeOfDayFrame todFrame = TimeOfDayFrame.Now(omcw, 0b111);
        transmitter.AddFrame(todFrame);

        var address = new Address(1, 0, 0, 0);
        var testPage = new PageBuilder(50, address, omcw)
            .AddLine("Hey this is page 50")
            .Build();
        
        while (true)
        {
            transmitter.AddFrame(testPage);
            Thread.Sleep(1000);
        }
    }
}