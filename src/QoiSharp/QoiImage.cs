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
    public byte[] Data { get; }
    
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
    {
        Data = data;
        Width = width;
        Height = height;
        Channels = channels;
        ColorSpace = colorSpace;
    }
}