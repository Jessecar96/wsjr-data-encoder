namespace JrEncoderLib;

public static class HammingCode
{
    /// <summary>
    /// Add error correcting bits to an input byte
    /// https://en.wikipedia.org/wiki/Hamming_code
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static byte Get(byte input)
    {
        return input switch
        {
            0 => 0x80,
            1 => 0x31,
            2 => 0x52,
            3 => 0xE3,
            4 => 0x64,
            5 => 0xD5,
            6 => 0xB6,
            7 => 0x7,
            8 => 0xF8,
            9 => 0x49,
            10 => 0x2A,
            11 => 0x9B,
            12 => 0x1C,
            13 => 0xAD,
            14 => 0xCE,
            15 => 0x7F,
            _ => 0x0
        };
    }
}