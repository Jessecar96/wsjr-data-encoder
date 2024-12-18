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
            .TopPage(0)
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

        // Build a TOD frame and send it
        TimeOfDayFrame todFrame = TimeOfDayFrame.Now(omcw, 8);
        transmitter.AddFrame(todFrame);

        DataFrame[] testPage = new PageBuilder(10, new Address(1, 2, 3, 4), omcw)
            .Attributes(new PageAttributes
            {
                Roll = true,
                Chain = true
            })
            .AddLine("This page will rolllllllllllll")
            .AddLine("This page will rolllllllllllll", new TextLineAttributes()
            {
                Border = true,
                Color = Color.Red,
                Height = 1,
                Width = 0,
            })
            .AddLine("This page will rolllllllllllll")
            .AddLine("This page will rolllllllllllll")
            .AddLine("This page will rolllllllllllll")
            .AddLine("This page will rolllllllllllll")
            .AddLine("This page will rolllllllllllll")
            .AddLine("This page will rolllllllllllll")
            .Build();
        transmitter.AddFrame(testPage);

        DataFrame[] testPage2 = new PageBuilder(11, new Address(1, 2, 3, 4), omcw)
            .Attributes(new PageAttributes
            {
                Roll = false
            })
            .AddLine("Page 11 rolllllllllllll")
            .AddLine("Page 11 rolllllllllllll")
            .AddLine("Page 11 rolllllllllllll")
            .AddLine("Page 11 rolllllllllllll")
            .AddLine("Page 11 rolllllllllllll")
            .AddLine("Page 11 rolllllllllllll")
            .AddLine("Page 11 rolllllllllllll")
            .AddLine("Page 11 rolllllllllllll")
            .Build();
        transmitter.AddFrame(testPage2);

        // Switch to page 11
        transmitter.omcw.TopPage(10).TopSolid().Commit();

        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}