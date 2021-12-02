namespace QoiSharp;

/// <summary>
/// Constants for encoder and decoder.
/// </summary>
public static class QoiCodec
{
    public const byte Index = 0x00;
    public const byte Run8 = 0x40;
    public const byte Run16 = 0x60;
    public const byte Diff8 = 0x80;
    public const byte Diff16 = 0xc0;
    public const byte Diff24 = 0xe0;

    public const byte Color = 0xf0;
    public const byte Mask2 = 0xc0;
    public const byte Mask3 = 0xe0;
    public const byte Mask4 = 0xf0;
    
    public const byte HeaderSize = 14;
    public const byte Padding = 4;
    public const string MagicString = "qoif";
    
    public static readonly int Magic = MagicString[0] << 24 | MagicString[1] << 16 | MagicString[2] << 8 | MagicString[3];
}