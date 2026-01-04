using System.Collections.Generic;
using System.Threading.Tasks;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

namespace JrEncoderLib.DataTransmitter;

public abstract class DataTransmitter(OMCW omcw)
{
    protected OMCW omcw = omcw;
    protected Queue<DataFrame> _frameQueue = [];

    /// <summary>
    /// Initialize this transmitter
    /// </summary>
    public abstract void Init();

    public abstract void Run();

    /// <summary>
    /// Add a DataFrame to the message queue, sends it out asap
    /// </summary>
    /// <param name="frame"></param>
    public void AddFrame(DataFrame frame)
    {
        _frameQueue.Enqueue(frame);
    }

    /// <summary>
    /// Add a DataFrame to the message queue, sends it out asap
    /// </summary>
    /// <param name="frames"></param>
    public void AddFrame(DataFrame[] frames)
    {
        foreach (DataFrame frame in frames)
        {
            _frameQueue.Enqueue(frame);
        }
    }
}