using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using System.Threading.Tasks;
using JrEncoderLib.Frames;
using JrEncoderLib.StarAttributes;

namespace JrEncoderLib.DataTransmitter;

public class GPIODataTransmitter(OMCW omcw) : DataTransmitter(omcw)
{
    private readonly GpioController _controller = new();
    private readonly Cipher _cipher = new();
    private SpiDevice? _ddsSpi;

    // Pin definitions //

    // Misc
    private const int RequestToSend = 2; // A7 RTS in Schematic   (Request to Send signal, Trigger, to transmit the FIFO contents to the receiver)
    private const int WriteEnable = 3; // A6 /W In Schematic    (Write strobe to insert 16-bits of data into the FIFO)
    private const int EmptyFifo = 4; // A5 /EF in Schematic   (Signal to indicate the FIFO is empty and ready for a new frame)
    private const int PeriphReset = 17; // A4 /RST in Schematic  (Resets both the FIFO, and DDS chip into initial states)
    private const int FLDirection = 27; // A3 FLDIR in Schematic (FIFO when in reset mode reads this for cascading, or MSB/LSB indication for data direction)
    private const int DdsFsync = 22; // FSYNC into DDS. Low when writing a 16 bit word, high when done.

    // Low byte (Fifolow)
    private const int LowDataBit0 = 0; // D0 / GPIO 0  / A8
    private const int LowDataBit1 = 5; // D1 / GPIO 5  / A7
    private const int LowDataBit2 = 6; // D2 / GPIO 6  / A6
    private const int LowDataBit3 = 13; // D3 / GPIO 13 / A5
    private const int LowDataBit4 = 19; // D4 / GPIO 19 / A4
    private const int LowDataBit5 = 20; // D5 / GPIO 20 / A2
    private const int LowDataBit6 = 26; // D6 / GPIO 26 / A3
    private const int LowDataBit7 = 21; // D7 / GPIO 21 / A1

    // High byte (Fifohigh)
    private const int HighDataBit0 = 14; // D8  / GPIO 14 / A1
    private const int HighDataBit1 = 15; // D9  / GPIO 15 / A2
    private const int HighDataBit2 = 18; // D10 / GPIO 18 / A3
    private const int HighDataBit3 = 23; // D11 / GPIO 23 / A4
    private const int HighDataBit4 = 24; // D12 / GPIO 24 / A5
    private const int HighDataBit5 = 25; // D13 / GPIO 25 / A6
    private const int HighDataBit6 = 1; // D14 / GPIO  1 / A7
    private const int HighDataBit7 = 12; // D15 / GPIO 12 / A8

    // Bool to store when the fifo is empty. This is updated from a callback on EmptyFifo 
    private bool _fifoIsEmpty;

    public override void Init()
    {
        // Open all pins we need //
        _controller.OpenPin(RequestToSend, PinMode.Output);
        _controller.OpenPin(WriteEnable, PinMode.Output);
        _controller.OpenPin(EmptyFifo, PinMode.Input);
        _controller.OpenPin(PeriphReset, PinMode.Output);
        _controller.OpenPin(FLDirection, PinMode.Output);
        _controller.OpenPin(DdsFsync, PinMode.Output);
        _controller.OpenPin(LowDataBit0, PinMode.Output);
        _controller.OpenPin(LowDataBit1, PinMode.Output);
        _controller.OpenPin(LowDataBit2, PinMode.Output);
        _controller.OpenPin(LowDataBit3, PinMode.Output);
        _controller.OpenPin(LowDataBit4, PinMode.Output);
        _controller.OpenPin(LowDataBit5, PinMode.Output);
        _controller.OpenPin(LowDataBit6, PinMode.Output);
        _controller.OpenPin(LowDataBit7, PinMode.Output);
        _controller.OpenPin(HighDataBit0, PinMode.Output);
        _controller.OpenPin(HighDataBit1, PinMode.Output);
        _controller.OpenPin(HighDataBit2, PinMode.Output);
        _controller.OpenPin(HighDataBit3, PinMode.Output);
        _controller.OpenPin(HighDataBit4, PinMode.Output);
        _controller.OpenPin(HighDataBit5, PinMode.Output);
        _controller.OpenPin(HighDataBit6, PinMode.Output);
        _controller.OpenPin(HighDataBit7, PinMode.Output);

        // Callback when the FIFO tells us it is empty, and can accept a new byte
        _controller.RegisterCallbackForPinValueChangedEvent(EmptyFifo, PinEventTypes.Falling,
            (sender, args) => { _fifoIsEmpty = true; });

        // Open SPI
        // A1/19 = MOSI
        // A1/23 = SCLK/SCK
        _ddsSpi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            DataFlow = DataFlow.MsbFirst,
            // Data line is idle low, data is sampled in on falling edge of clock
            Mode = SpiMode.Mode2
        });

        // Program the DDS chip (twice because it likes that)
        InitDds();
        InitDds();
        Console.WriteLine("DDS Init Complete");
    }

    public override void Run()
    {
        DataFrame[] idleFrames = new PageBuilder(42, Address.None, omcw)
            .AddLine("00000000000000000000000000000000")
            .Build();

        while (true)
        {
            if (_frameQueue.Count == 0)
            {
                // Message queue is empty, send idle frame
                WriteFrames(idleFrames);
            }
            else
            {
                // We have a message to send, send it
                WriteFrame(_frameQueue.Dequeue().GetBytes());
            }
        }
    }

    /**
     * Reset DDS chip and send settings to it
     */
    private void InitDds()
    {
        /*
         * 26771963   7.48Mhz Mark Frequency.  00011001100010 00000111111011 **
         * 26590036   7.43Mhz Mark Frequency.  00011001010110 11101101010100
         * 26235092   7.33Mhz Space Frequency. 00011001000001 01000011010100
         * 26056135   7.28Mhz Space Frequency. 00011000110110 01010111000111 **
         */

        if (_ddsSpi == null)
            throw new Exception("DDS SPI was not initialized");
        
        // The FIFO wants write high during reset 
        _controller.Write(WriteEnable, PinValue.High);

        // Set DDS to RESET. It's active high and we invert this signal (stops any output and resets all registers)
        // This also sets the FIFO into RESET
        Console.WriteLine("Resetting DDS");
        _controller.Write(PeriphReset, PinValue.Low);
        
        // Set the FIFO "First Load" pin to low, this tells it that it is the first device in a chain (if we had multiple of them)
        _controller.Write(FLDirection, PinValue.Low);

        // Good guide: https://dk7ih.de/programming-the-ad9834-dds-chip/
        // Set initial RESET state of DDS
        // It starts with 00 to signal a "control word"
        // DB13 is set to 1 to tell it that we intend to send both LSB and MSB values consecutively
        // DB9 is set to 1 to tell it that we want to select the correct frequency with hardware and not via serial
        _controller.Write(DdsFsync, PinValue.Low);
        _ddsSpi.WriteByte(0b00100010); // 0010 0010 0000 0000
        _ddsSpi.WriteByte(0b00000000);
        _controller.Write(DdsFsync, PinValue.High);

        Console.WriteLine("Writing frequencies");

        // Set Space Frequency (FREQ0) Lower 14 Bits (LSB) (when FS is high)
        // 01010111000111
        _controller.Write(DdsFsync, PinValue.Low);
        _ddsSpi.WriteByte(0b01010101);
        _ddsSpi.WriteByte(0b11000111);
        _controller.Write(DdsFsync, PinValue.High);

        // Set Space Frequency (FREQ0) Upper 14 Bits (MSB)
        // 00011000110110
        _controller.Write(DdsFsync, PinValue.Low);
        _ddsSpi.WriteByte(0b01000110);
        _ddsSpi.WriteByte(0b00110110);
        _controller.Write(DdsFsync, PinValue.High);


        // Set Mark Frequency (FREQ1) Lower 14 Bits (when FS is low)
        // 00000111111011
        _controller.Write(DdsFsync, PinValue.Low);
        _ddsSpi.WriteByte(0b10000001);
        _ddsSpi.WriteByte(0b11111011);
        _controller.Write(DdsFsync, PinValue.High);

        // Set Mark Frequency (FREQ1) Upper 14 Bits
        // 00011001100010
        _controller.Write(DdsFsync, PinValue.Low);
        _ddsSpi.WriteByte(0b10000110);
        _ddsSpi.WriteByte(0b01100010);
        _controller.Write(DdsFsync, PinValue.High);

        // Turn off DDS & FIFO RESET, this enables the output of it
        _controller.Write(PeriphReset, PinValue.High);
        
        // Once out of RESET the FLDIR pin on the FIFO changes from "First Load" into DIRECTION
        // Low/High makes it clock out in MSB or LSB first
        _controller.Write(FLDirection, PinValue.Low);
        
        Console.WriteLine("DDS Reset Complete");
    }

    /// <summary>
    /// Write multiple data frames to the modulator
    /// </summary>
    /// <param name="frames"></param>
    private void WriteFrames(DataFrame[] frames)
    {
        foreach (DataFrame frame in frames)
        {
            WriteFrame(frame.GetBytes());
        }
    }

    /// <summary>
    /// Write a single DataFrame to the modulator
    /// </summary>
    /// <param name="frame"></param>
    private void WriteFrame(byte[] frame)
    {
        // Write Zeros into the FIFO for the "wrap-around" bug, and to reset IV to 0.
        _cipher.ResetFields();

        for (int i = 0; i < frame.Length; i += 2)
        {
            byte loByte = frame[i];
            WriteLowByte(_cipher.EncryptByte(loByte));

            byte hiByte = frame[i + 1];
            WriteHighByte(_cipher.EncryptByte(hiByte));

            StrobeFifo(); // Tell the FIFO to read in a byte
        }

        // Write Zeros into the FIFO for the "wrap-around" bug, and to reset IV to 0.
        WriteLowByte(0);
        WriteHighByte(0);
        StrobeFifo();

        // Tell the FIFO to write out its stored data
        _controller.Write(RequestToSend, PinValue.High);
        _controller.Write(RequestToSend, PinValue.Low);

        while (!_fifoIsEmpty)
        {
            // Wait here until we get the callback from the fifo being empty
        }

        // Reset this bool for the next time
        _fifoIsEmpty = false;

        // DEBUG: Write packets to console
        //Console.WriteLine(Convert.ToHexString(frame));
    }

    private void StrobeFifo()
    {
        _controller.Write(WriteEnable, PinValue.Low);
        _controller.Write(WriteEnable, PinValue.High);
    }

    private void WriteLowByte(byte value)
    {
        _controller.Write(LowDataBit0, value & 0b00000001);
        _controller.Write(LowDataBit1, value & 0b00000010);
        _controller.Write(LowDataBit2, value & 0b00000100);
        _controller.Write(LowDataBit3, value & 0b00001000);
        _controller.Write(LowDataBit4, value & 0b00010000);
        _controller.Write(LowDataBit5, value & 0b00100000);
        _controller.Write(LowDataBit6, value & 0b01000000);
        _controller.Write(LowDataBit7, value & 0b10000000);
    }

    private void WriteHighByte(byte value)
    {
        _controller.Write(HighDataBit0, value & 0b00000001);
        _controller.Write(HighDataBit1, value & 0b00000010);
        _controller.Write(HighDataBit2, value & 0b00000100);
        _controller.Write(HighDataBit3, value & 0b00001000);
        _controller.Write(HighDataBit4, value & 0b00010000);
        _controller.Write(HighDataBit5, value & 0b00100000);
        _controller.Write(HighDataBit6, value & 0b01000000);
        _controller.Write(HighDataBit7, value & 0b10000000);
    }
}