namespace SharpQoi;

internal static class QoiConsts
{
    public const byte Index = 0x00; // 00xxxxxx
    public const byte Run8 = 0x40; // 010xxxxx
    public const byte Run16 = 0x60; // 011xxxxx
    public const byte Diff8 = 0x80; // 10xxxxxx
    public const byte Diff16 = 0xc0; // 110xxxxx
    public const byte Diff24 = 0xe0; // 1110xxxx

    public const byte Color = 0xf0; // 1111xxxx
    public const byte Mask2 = 0xc0; // 11000000
    public const byte Mask3 = 0xe0; // 11100000
    public const byte Mask4 = 0xf0; // 11110000
    
    public const byte HeaderSize = 14;
    public const byte Padding = 4;
    public const string MagicString = "qoif";
    
    public static readonly int Magic = MagicString[0] << 24 | MagicString[1] << 16 | MagicString[2] << 8 | MagicString[3];
}