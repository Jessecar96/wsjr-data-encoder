using System;

namespace JrEncoderLib.StarAttributes;

/// <summary>
/// Class that handles the construction and manipulation of the Output Mode Control Word.
/// </summary>
public class OMCW
{
    // Instance variables. See their respective setter functions for details.
    private bool _localProgram;
    private bool _localPreroll;
    private bool _auxAudio;
    private bool _wxWarning;
    private bool _radar;
    private bool _regionSeparator;
    private bool _topSolid;
    private bool _botSolid;
    private int _topPageNum;
    private LDLStyle _ldlStyle = LDLStyle.Nothing;

    private bool _needsCommit = false;

    /// <summary>
    /// Bytes 4-9 that will be manipulated. They are attached to header packets.
    /// </summary>
    private readonly byte[] _omcwBytes = new byte[6];

    public static OMCW Create()
    {
        return new OMCW();
    }

    /// <summary>
    /// Enables or disables local program.
    /// </summary>
    /// <param name="enabled">To enable or not.</param>
    /// <returns></returns>
    public OMCW LocalProgram(bool enabled = true)
    {
        _localProgram = enabled;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Enables or disables local preroll.
    /// </summary>
    /// <param name="enabled">To enable or not.</param>
    /// <returns></returns>
    public OMCW LocalPreroll(bool enabled = true)
    {
        _localPreroll = enabled;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Enables or disables the alternate audio stream.
    /// </summary>
    /// <param name="enabled">To enable or not.</param>
    /// <returns></returns>
    public OMCW AuxAudio(bool enabled = true)
    {
        _auxAudio = enabled;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Enables or disables the "WX WARNING" relay.
    /// </summary>
    /// <param name="enabled">Relay will be opened or not.</param>
    /// <returns></returns>
    public OMCW WxWarning(bool enabled = true)
    {
        _wxWarning = !enabled;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Enables or disables the local video feed for the radar.
    /// </summary>
    /// <param name="enabled">To enable or not.</param>
    /// <returns></returns>
    public OMCW Radar(bool enabled = true)
    {
        _radar = enabled;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Enables or disables the LDL Separator.
    /// </summary>
    /// <param name="enabled">Whether the LDL separator will be shown or not.</param>
    /// <returns></returns>
    public OMCW RegionSeparator(bool enabled = true)
    {
        _regionSeparator = enabled;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Enables or disables drawing an opaque background on the top region.
    /// </summary>
    /// <param name="solid">Whether the top region will be opaque.</param>
    /// <returns></returns>
    public OMCW TopSolid(bool solid = true)
    {
        _topSolid = solid;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Enables or disables drawing an opaque background on the bottom region.
    /// </summary>
    /// <param name="solid">Whether the bottom region will be opaque.</param>
    /// <returns></returns>
    public OMCW BottomSolid(bool solid = true)
    {
        _botSolid = solid;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Sets the page to display in the upper region.
    /// </summary>
    /// <param name="pageNumber">The page number to display.</param>
    /// <returns></returns>
    public OMCW TopPage(int pageNumber)
    {
        _topPageNum = pageNumber;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Sets the mode to display the LDL in.
    /// </summary>
    /// <param name="style">The style to display the LDL with.</param>
    /// <returns></returns>
    public OMCW LDL(LDLStyle style)
    {
        _ldlStyle = style;
        _needsCommit = true;
        return this;
    }

    /// <summary>
    /// Commits any changes to the internal values to the byte format returned when calling <see cref="ToBytes"/>.
    /// </summary>
    /// <returns></returns>
    public OMCW Commit()
    {
        // Byte 4
        // We're shifting these bits to the left, then using bitwise or to properly setup the nibbles.
        // If parameter is true, set variable to 1. Else, set it to 0. Then shift to the left by x for bitwise operations.
        // ((parameter) ? 1 : 0) << x;
        int localProgram = (_localProgram ? 1 : 0) << 3;
        int localPreroll = (_localPreroll ? 1 : 0) << 2;
        int auxAudio = (_auxAudio ? 1 : 0) << 1;
        int wxWarning = _wxWarning ? 1 : 0; // No need to shift as it's already at position 0.
        _omcwBytes[0] = (byte)(localProgram | localPreroll | auxAudio | wxWarning); //Generate the nibble.

        // Byte 5
        int radar = (_radar ? 1 : 0) << 3;
        int regionSeparator = (_regionSeparator ? 1 : 0) << 2;
        int topSolid = (_topSolid ? 1 : 0) << 1;
        int botSolid = _botSolid ? 1 : 0; // No need to shift as it's already at position 0.
        _omcwBytes[1] = (byte)(radar | regionSeparator | topSolid | botSolid); //Generate the nibble.

        // Byte 6.
        // MASK: 0x3C = 111100... we're only wanting the upper four bits of the topPageNum variable.
        // Then shift right by two to make room for hamming.
        _omcwBytes[2] = (byte)((_topPageNum & 0b00111100) >> 2);

        // Byte 7
        // Get the two least significant bits of topPageNumber, shift by 2, then place LDL page number in the two least significant bits to create the nibble.
        _omcwBytes[3] = (byte)(((_topPageNum & 0b00000011) << 2) | ((byte)_ldlStyle & 0b00000011));

        // Byte 8
        _omcwBytes[4] = (byte)(_topPageNum & 0b11110000); // Top Page Number MSB

        // Byte 9
        _omcwBytes[5] = (byte)(_topPageNum & 0b00001111); // Top Page Number LSB

        _needsCommit = false;
        return this;
    }

    /// <summary>
    /// The byte representation of this OMCW.
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        // Wait until _needsCommit is no longer true
        while (_needsCommit)
        {
            Console.WriteLine("WARNING: OMCW being used without first committing");
            // Wait
        }

        return _omcwBytes;
    }
}