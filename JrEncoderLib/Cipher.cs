namespace JrEncoderLib;

// Original work was done by Mike Bates (techknight)
// This is the object-oriented approach using C#/.NET Standard to encode the ciphertext expected of the legacy WeatherStars that use FSK
public class Cipher
{
    public int Iv, ShiftIvCount;

    /// <summary>
    /// This function performs a bit-level XOR encryption using a 21-bit IV Work register.
    /// </summary>
    /// <param name="dataByte"></param>
    /// <returns></returns>
    public byte EncryptByte(byte dataByte) // iv and shiftIvCount can be taken from the fields of the client code
    {
        int endByte = 0;
        byte shiftByte = dataByte; // Store our plaintext byte to be enciphered
        for (int i = 0; i < 8; i++) // Loop through 8 bits
        {
            byte bitB; // within inner scope
            byte carryFlag = (byte)(shiftByte & 1); // Shift our LSB into the Carry flag
            shiftByte = (byte)(shiftByte >> 1); // Store ONLY the Carry flag
            byte bitA = (byte)((Iv & 4) / 4 ^ (Iv & 524288) / 524288); // XOR IV Work Register Bits 2, and 19
            byte countSet = (byte)(Iv & 1 ^ (Iv & 256) / 256); // XOR IV Work Register Bits 0, and 8.
            if (countSet == 1) // if Previous XOR operation resulted in a high, Reset the counter
                ShiftIvCount = 0;

            if ((ShiftIvCount & 31) == 31) // Store only the first 5 bits of our count register
            {
                bitB = (byte)(bitA ^ 0);
            }
            else // if all 5 bits are a 1, Our counter has tripped, so we need to set the flag as a 0
            {
                bitB = (byte)(bitA ^ 1);
            }

            byte bitC = (byte)((carryFlag & 1) ^ bitB); // XOR our Key bit with our Carry (data) Bit.
            endByte = (byte)(endByte >> 1); // Shift our ciphertext by 1
            Iv = Iv << 1; // Shift our IV Work register by 1
            ShiftIvCount++; // Increment our counter by 1

            Iv = (Iv | bitC); // Store our result into the LSB of our ciphertext byte
            bitC = (byte)(bitC << 7); // get bitC into the 8th bit position
            endByte = (byte)(endByte | bitC); // Store our result into the LSB of our IV work register
        }

        return (byte)endByte;
    }

    public void ResetFields()
    {
        Iv = ShiftIvCount = 0;
    }
}