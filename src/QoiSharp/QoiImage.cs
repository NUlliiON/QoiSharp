namespace QoiSharp;

/// <summary>
/// QOI image.
/// </summary>
public class QoiImage
{
    public byte[] Pixels { get; }
    
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
    public byte Channels { get; }
    
    /// <summary>
    /// Color space.
    /// </summary>
    public ColorSpace ColorSpace { get; }
    
    /// <summary>
    /// Default constructor.
    /// </summary>
    public QoiImage(byte[] pixels, int width, int height, byte channels, ColorSpace colorSpace = ColorSpace.SRgb)
    {
        Pixels = pixels;
        Width = width;
        Height = height;
        Channels = channels;
        ColorSpace = colorSpace;
    }
}