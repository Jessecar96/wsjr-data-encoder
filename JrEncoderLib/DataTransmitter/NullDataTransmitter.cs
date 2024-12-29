using System.Threading.Tasks;
using JrEncoderLib.StarAttributes;

namespace JrEncoderLib.DataTransmitter;

/// <summary>
/// Doesn't do anything with the data, for testing only
/// </summary>
public class NullDataTransmitter(OMCW omcw) : DataTransmitter(omcw)
{
    public override void Init()
    {
    }

    public override Task Run()
    {
        while (true) Task.Delay(100);
    }
}