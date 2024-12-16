namespace JrEncoder;

using System.Device.Gpio;

class Program
{
    static void Main(string[] args)
    {
        DataTransmitter transmitter = new();
        transmitter.Init();
    }
}