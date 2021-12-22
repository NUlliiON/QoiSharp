namespace QoiSharp.Codec;

public enum Channels : byte
{
    /// <summary>
    /// 3-channel format containing data for Red, Green, Blue.
    /// </summary>
    Rgb = 3,
    
    /// <summary>
    /// 4-channel format containing data for Red, Green, Blue, and Alpha.
    /// </summary>
    RgbWithAlpha = 4
}