using JrEncoder.StarAttributes;

namespace JrEncoder.Frames;

public class PageHeaderFrame : ControlFrame
{
    public PageHeaderFrame(int pageNumber, int lineCount, OMCW omcw, Address address, PageAttributes attributes,
                           TextLineAttributes line1attributes, TextLineAttributes line2attributes,
                           TextLineAttributes line3attributes, TextLineAttributes line4attributes,
                           TextLineAttributes line5attributes, TextLineAttributes line6attributes,
                           TextLineAttributes line7attributes, TextLineAttributes line8attributes) {
        this.omcw = omcw;

        /*
        Prior Reference: https://patentimages.storage.googleapis.com/6d/b2/60/69fee298647dc3/US4725886.pdf
        Reference: https://patentimages.storage.googleapis.com/8d/f3/42/7f8952923cce48/US4916539.pdf
        */

        byte[] attributeBytes = attributes.ToBytes();

        frame[0] = ClockRunIn;
        frame[1] = ClockRunIn;
        frame[2] = FramingCode;
        frame[3] = 0;  // Row Number: 0 for page header
        // 4-7 OMCW
        frame[8] = (byte) ((pageNumber >> 4) & 0x0F);   // Page Number
        frame[9] = (byte) (pageNumber & 0x0F);          // "
        frame[10] = 2; // Address
        frame[11] = 0; // "
        frame[12] = 0; // "
        frame[13] = 0; // "
        frame[14] = 0; // "
        frame[15] = 0; // "
        frame[16] = (byte) lineCount;   // Line Count
        frame[17] = attributeBytes[0];  // Page Attributes
        frame[18] = attributeBytes[1];  // "
        frame[19] = line1attributes.GetByte1(); // Line 1 Attributes
        frame[20] = line1attributes.GetByte2(); // "
        frame[21] = line2attributes.GetByte1(); // Line 2 Attributes
        frame[22] = line2attributes.GetByte2(); // "
        frame[23] = line3attributes.GetByte1(); // Line 3 Attributes
        frame[24] = line3attributes.GetByte2(); // "
        frame[25] = line4attributes.GetByte1(); // Line 4 Attributes
        frame[26] = line4attributes.GetByte2(); // "
        frame[27] = line5attributes.GetByte1(); // Line 5 Attributes
        frame[28] = line5attributes.GetByte2(); // "
        frame[29] = line6attributes.GetByte1(); // Line 6 Attributes
        frame[30] = line6attributes.GetByte2(); // "
        frame[31] = line7attributes.GetByte1(); // Line 7 Attributes
        frame[32] = line7attributes.GetByte2(); // "
        frame[33] = 0; // OMCW Extension
        frame[34] = 0; // "
        frame[35] = line8attributes.GetByte1(); // Line 8/9 Attributes
        frame[36] = line8attributes.GetByte2(); // "

        // Set bytes 4-7 for OMCW
        setOmcwBytes();

        // TODO: Set address

        // Convert bytes 3-36 to hamming code
        hamBytes(3, 36);

    }
}