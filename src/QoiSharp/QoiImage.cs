using QoiSharp.Codec;

namespace QoiSharp;

/// <summary>
/// QOI image.
/// </summary>
public class QoiImage
{
    /// <summary>
    /// Raw pixel data.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    // Make the zero-copy version available internally for safety. The public API clones the byte[] to guarantee immutability.
    internal static QoiImage FromMemory(ReadOnlyMemory<byte> memory, int width, int height, Channels channels, ColorSpace colorSpace = ColorSpace.SRgb)
        => new QoiImage(memory, width, height, channels, colorSpace);

    /// <summary>
    /// Image width.
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// Image height
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// Channels.
    /// </summary>
    public Channels Channels { get; }
    
    /// <summary>
    /// Color space.
    /// </summary>
    public ColorSpace ColorSpace { get; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public QoiImage(byte[] data, int width, int height, Channels channels, ColorSpace colorSpace = ColorSpace.SRgb)
        : this(new ReadOnlyMemory<byte>((byte[])data.Clone()), width, height, channels, colorSpace)
    {

    }

    QoiImage(ReadOnlyMemory<byte> memory, int width, int height, Channels channels, ColorSpace colorSpace = ColorSpace.SRgb)
    {
        Data = memory;
        Width = width;
        Height = height;
        Channels = channels;
        ColorSpace = colorSpace;
    }
}