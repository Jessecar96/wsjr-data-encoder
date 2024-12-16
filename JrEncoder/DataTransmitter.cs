using System.Collections;
using System.Device.Gpio;
using System.Device.Spi;
using System.Text;

namespace JrEncoder;

public class DataTransmitter
{
    private GpioController controller = new();
    private SpiDevice ddsSpi;
    private Cipher cipher = new();

    // Pin definitions //

    // Misc
    private const int RequestToSend = 2; // A7 RTS in Schematic   (Request to Send signal, Trigger, to transmit the FIFO contents to the receiver)
    private const int WriteEnable = 3;   // A6 /W In Schematic    (Write strobe to insert 16-bits of data into the FIFO)
    private const int EmptyFifo = 4;     // A5 /EF in Schematic   (Signal to indicate the FIFO is empty and ready for a new frame)
    private const int PeriphReset = 17;  // A4 /RST in Schematic  (Resets both the FIFO, and DDS chip into initial states)
    private const int Direction = 27;    // A3 FLDIR in Schematic (FIFO when in reset mode reads this for cascading, or MSB/LSB indication for data direction)
    private const int DdsFsync = 22;     // FSYNC into DDS. Low when writing a 16 bit word, high when done.
    
    // Low byte (Fifolow)
    private const int LowDataBit0 = 0; // GPIO 0 / A8
    private const int LowDataBit1 = 5; // GPIO 5 / A7
    private const int LowDataBit2 = 6; // GPIO 6 / A6
    private const int LowDataBit3 = 13; // GPIO 13 / A5
    private const int LowDataBit4 = 19; // GPIO 19 / A4
    private const int LowDataBit5 = 26; // GPIO 26 / A3
    private const int LowDataBit6 = 20; // GPIO 20 / A2
    private const int LowDataBit7 = 21; // GPIO 21 / A1

    // High byte (Fifohigh)
    private const int HighDataBit0 = 14; // D8  / GPIO 14 / A1
    private const int HighDataBit1 = 15; // D8  / GPIO 15 / A2
    private const int HighDataBit2 = 18; // D10 / GPIO 18 / A3
    private const int HighDataBit3 = 23; // D11 / GPIO 23 / A4
    private const int HighDataBit4 = 24; // D12 / GPIO 24 / A5
    private const int HighDataBit5 = 25; // D13 / GPIO 25 / A6
    private const int HighDataBit6 = 1;  // D14 / GPIO  1 / A7
    private const int HighDataBit7 = 12; // D15 / GPIO 12 / A8
    
    // Bool to store when the fifo is empty. This is updated from a callback on EmptyFifo 
    private bool _fifoIsEmpty;

    public void Init()
    {
        Console.WriteLine("Starting WeatherSTAR Data Transmitter");
        Console.WriteLine("Written by Jesse Cardone, 2024");
        
        // Open all pins we need //
        controller.OpenPin(RequestToSend, PinMode.Output);
        controller.OpenPin(WriteEnable, PinMode.Output);
        controller.OpenPin(EmptyFifo, PinMode.Input);
        controller.OpenPin(PeriphReset, PinMode.Output);
        controller.OpenPin(Direction, PinMode.Output);
        controller.OpenPin(DdsFsync, PinMode.Output);
        controller.OpenPin(LowDataBit0, PinMode.Output);
        controller.OpenPin(LowDataBit1, PinMode.Output);
        controller.OpenPin(LowDataBit2, PinMode.Output);
        controller.OpenPin(LowDataBit3, PinMode.Output);
        controller.OpenPin(LowDataBit4, PinMode.Output);
        controller.OpenPin(LowDataBit5, PinMode.Output);
        controller.OpenPin(LowDataBit6, PinMode.Output);
        controller.OpenPin(LowDataBit7, PinMode.Output);
        controller.OpenPin(HighDataBit0, PinMode.Output);
        controller.OpenPin(HighDataBit1, PinMode.Output);
        controller.OpenPin(HighDataBit2, PinMode.Output);
        controller.OpenPin(HighDataBit3, PinMode.Output);
        controller.OpenPin(HighDataBit4, PinMode.Output);
        controller.OpenPin(HighDataBit5, PinMode.Output);
        controller.OpenPin(HighDataBit6, PinMode.Output);
        controller.OpenPin(HighDataBit7, PinMode.Output);

        // Initial setup
        controller.Write(RequestToSend, PinValue.Low); // Reading not allowed
        controller.Write(WriteEnable, PinValue.Low); // Write not allowed
        controller.Write(PeriphReset, PinValue.High); // High = enable, low = RESET
        controller.Write(Direction, PinValue.Low); // Set our chip in to single mode

        controller.RegisterCallbackForPinValueChangedEvent(EmptyFifo, PinEventTypes.Falling, (sender, args) =>
        {
            //Console.WriteLine("Empty Fifo received");
            _fifoIsEmpty = true;
        });
        
        // Open SPI
        // A1/19 = MOSI
        // A1/23 = SCLK/SCK
        ddsSpi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            DataFlow = DataFlow.MsbFirst,
            // Data line is idle low, data is sampled in on falling edge of clock
            Mode = SpiMode.Mode2
        });

        InitDds();
        Console.WriteLine("DDS Init Complete");
        
        DateTime currentTime = DateTime.Now;
        int hour = int.Parse(currentTime.ToString("%h"));
        int amPm = currentTime.Hour > 12 ? 1 : 0;
        
        // First OMCW, show only LDL
        OMCW omcw = new(false, false, false, false, false, true, true, true, 10, 3);
        TimeOfDayFrame todFrame = new(omcw, 8, 1, currentTime.Month, currentTime.Day, hour, currentTime.Minute, currentTime.Second, amPm);
        
        DataFrame[] testPage = new PageBuilder(10)
            .setOMCW(omcw)
            .setPageAttributes(new PageAttributes(false, false, false, false, false, false))
            .setLineAttributes(
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue)
            )
            .addLine(1, "Conditions on Earth")
            .addLine(2, "Cataclysm")
            .addLine(3, "Temp: 0K")
            .addLine(4, "Humidity: -51%   Dewpoint: yes")
            .addLine(5, "Barometric Pressure: 753 atm.")
            .addLine(6, "Wind: DOWN 7 LYPH")
            .addLine(7, "Visib: 0 in. Ceiling: html")
            .addLine(8, "9 + 10: 21")

            .setAddress(new Address(1, 2, 3, 4))
            .build();
        
        DataFrame[] ldlPage = new PageBuilder(50)
            .setOMCW(omcw)
            .setPageAttributes(new PageAttributes(false, false, false, false, false, false))
            .setLineAttributes(
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue)
            )
            .addLine(1, "This should show on the LDL :)")
            .addLine(2, "This should also work too")
            .addLine(3, "LDL line 3")
            .setAddress(new Address(1, 2, 3, 4))
            .build();
        
        WriteFrame(todFrame.getFrame());
        
        foreach (DataFrame frame in ldlPage)
        {
            WriteFrame(frame.getFrame());
        }
        
        while (true)
        {
            // Idle frame
            foreach (DataFrame frame in testPage)
            {
                WriteFrame(frame.getFrame());
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
        
        // Ensure FIFO is set up as single only
        //controller.Write(Direction, PinValue.High);
        
        // Set DDS to RESET. It's active high and we invert this signal (stops any output and resets all registers)
        Console.WriteLine("Resetting DDS");
        controller.Write(PeriphReset, PinValue.Low);
        
        // Give it 1ms to catch up
        Thread.Sleep(10);
        
        // Good guide: https://dk7ih.de/programming-the-ad9834-dds-chip/
        // Set initial RESET state of DDS
        // It starts with 00 to signal a "control word"
        // DB13 is set to 1 to tell it that we intend to send both LSB and MSB values consecutively
        // DB9 is set to 1 to tell it that we want to select the correct frequency with hardware and not via serial
        controller.Write(DdsFsync, PinValue.Low);
        ddsSpi.WriteByte(0b00100010); // 0010 0010 0000 0000
        ddsSpi.WriteByte(0b00000000); 
        controller.Write(DdsFsync, PinValue.High);
        
        // Give it 1ms to catch up
        Thread.Sleep(1);
        
        // Give it 1ms to catch up
        Thread.Sleep(1000);
        Console.WriteLine("Writing frequencies");
        
        
        // Set Space Frequency (FREQ0) Lower 14 Bits (LSB) (when FS is high)
        // 01010111000111
        controller.Write(DdsFsync, PinValue.Low);
        ddsSpi.WriteByte(0b01010101);
        ddsSpi.WriteByte(0b11000111);
        controller.Write(DdsFsync, PinValue.High);
        
        // Set Space Frequency (FREQ0) Upper 14 Bits (MSB)
        // 00011000110110
        controller.Write(DdsFsync, PinValue.Low);
        ddsSpi.WriteByte(0b01000110);
        ddsSpi.WriteByte(0b00110110);
        controller.Write(DdsFsync, PinValue.High);
        
        
        
        // Set Mark Frequency (FREQ1) Lower 14 Bits (when FS is low)
        // 00000111111011
        controller.Write(DdsFsync, PinValue.Low);
        ddsSpi.WriteByte(0b10000001);
        ddsSpi.WriteByte(0b11111011);
        controller.Write(DdsFsync, PinValue.High);
        
        // Set Mark Frequency (FREQ1) Upper 14 Bits
        // 00011001100010
        controller.Write(DdsFsync, PinValue.Low);
        ddsSpi.WriteByte(0b10000110);
        ddsSpi.WriteByte(0b01100010);
        controller.Write(DdsFsync, PinValue.High);
        
        // Give it 1ms to catch up
        Thread.Sleep(1);
        
        // Turn off DDS RESET, this enables the output of it
        controller.Write(PeriphReset, PinValue.High);
        
        // Give it 1ms to catch up
        Thread.Sleep(1);
        
        // Make sure our FIFO clocks out in the correct direction
        //controller.Write(Direction, PinValue.Low); 
        Console.WriteLine("DDS Reset Complete");
    }

    private void WriteFrame(byte[] frame)
    {
        // Write Zeros into the FIFO for the "wrap-around" bug, and to reset IV to 0.
        cipher.ResetFields();
        
        for (int i = 0; i < frame.Length; i+=2)
        {
            byte loByte = frame[i];
            WriteLowByte(cipher.EncryptByte(loByte));
            
            byte hiByte = frame[i+1];
            WriteHighByte(cipher.EncryptByte(hiByte));
            
            StrobeFifo(); // Tell the FIFO to read in a byte
        }
        
        // Write Zeros into the FIFO for the "wrap-around" bug, and to reset IV to 0.
        WriteLowByte(0);
        WriteHighByte(0);
        StrobeFifo();
        
        // Tell the FIFO to write out its stored data
        controller.Write(RequestToSend, PinValue.High);
        controller.Write(RequestToSend, PinValue.Low);
        
        while (!_fifoIsEmpty)
        {
            //Thread.Sleep(1);
        }

        // Reset this bool for the next time
        _fifoIsEmpty = false;
        
        // Wait until FIFO Empty (End of Transmission)
        //controller.WaitForEvent(EmptyFifo, PinEventTypes.Falling, TimeSpan.FromSeconds(1));
        
        // Needs a bit of a delay after each frame or else the LDL bounces
        //Thread.Sleep(15);
        
        //Console.WriteLine(Convert.ToHexString(frame));
    }

    private void StrobeFifo()
    {
        controller.Write(WriteEnable, PinValue.Low);
        controller.Write(WriteEnable, PinValue.High);
    }
    
    private void WriteLowByte(byte value)
    {
        controller.Write(LowDataBit0, value & 0b00000001);
        controller.Write(LowDataBit1, value & 0b00000010);
        controller.Write(LowDataBit2, value & 0b00000100);
        controller.Write(LowDataBit3, value & 0b00001000);
        controller.Write(LowDataBit4, value & 0b00010000);
        controller.Write(LowDataBit5, value & 0b00100000);
        controller.Write(LowDataBit6, value & 0b01000000);
        controller.Write(LowDataBit7, value & 0b10000000);
    }

    private void WriteHighByte(byte value)
    {
        controller.Write(HighDataBit0, value & 0b00000001);
        controller.Write(HighDataBit1, value & 0b00000010);
        controller.Write(HighDataBit2, value & 0b00000100);
        controller.Write(HighDataBit3, value & 0b00001000);
        controller.Write(HighDataBit4, value & 0b00010000);
        controller.Write(HighDataBit5, value & 0b00100000);
        controller.Write(HighDataBit6, value & 0b01000000);
        controller.Write(HighDataBit7, value & 0b10000000);
    }
}