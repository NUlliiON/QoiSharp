using QoiSharp.Codec;
using QoiSharp.Exceptions;

using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace QoiSharp;

/// <summary>
/// QOI encoder.
/// </summary>
public static class QoiEncoder
{
    /// <summary>
    /// Encodes raw pixel data into QOI.
    /// </summary>
    /// <param name="image">QOI image.</param>
    /// <returns>Encoded image.</returns>
    /// <exception cref="QoiEncodingException">Thrown when image information is invalid.</exception>
    public static byte[] Encode(QoiImage image)
    {
        var bytes = new byte[QoiCodec.HeaderSize + QoiCodec.Padding.Length + (image.Width * image.Height * (byte)image.Channels)];
        return bytes[..Encode(image, bytes)];
    }

    public static int Encode(QoiImage image, Span<byte> buffer)
    {
        if (image.Width == 0)
        {
            throw new QoiEncodingException($"Invalid width: {image.Width}");
        }

        if (image.Height == 0 || image.Height >= QoiCodec.MaxPixels / image.Width)
        {
            throw new QoiEncodingException($"Invalid height: {image.Height}. Maximum for this image is {QoiCodec.MaxPixels / image.Width - 1}");
        }

        int width = image.Width;
        int height = image.Height;
        byte channels = (byte)image.Channels;
        byte colorSpace = (byte)image.ColorSpace;
        ReadOnlySpan<byte> pixels = image.Data.Span;

        if (buffer.Length < QoiCodec.HeaderSize + QoiCodec.Padding.Length + (width * height * channels))
            return -1;

        BinaryPrimitives.WriteInt32BigEndian(buffer, QoiCodec.Magic);
        BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(4), width);
        BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(8), height);

        buffer[12] = channels;
        buffer[13] = colorSpace;

        byte[] index = new byte[QoiCodec.HashTableSize * 4];

        Span<byte> prev = stackalloc byte[4];
        prev.Clear();
        prev[3] = 255;

        Span<byte> rgba = stackalloc byte[4];
        prev.CopyTo(rgba);

        Span<int> prevAsInt = MemoryMarshal.Cast<byte, int>(prev);
        Span<int> rgbaAsInt = MemoryMarshal.Cast<byte, int>(rgba);

        int run = 0;
        int p = QoiCodec.HeaderSize;
        bool hasAlpha = channels == 4;

        int pixelsLength = width * height * channels;
        int pixelsEnd = pixelsLength - channels;
        int counter = 0;

        for (int pxPos = 0; pxPos < pixelsLength; pxPos += channels)
        {
            rgba[0] = pixels[pxPos];
            rgba[1] = pixels[pxPos + 1];
            rgba[2] = pixels[pxPos + 2];
            if (hasAlpha)
            {
                rgba[3] = pixels[pxPos + 3];
            }

            if (prevAsInt[0] == rgbaAsInt [0])
            {
                run++;
                if (run == 62 || pxPos == pixelsEnd)
                {
                    buffer[p++] = (byte)(QoiCodec.Run | (run - 1));
                    run = 0;
                }
            }
            else
            {
                if (run > 0)
                {
                    buffer[p++] = (byte)(QoiCodec.Run | (run - 1));
                    run = 0;
                }

                int indexPos = QoiCodec.CalculateHashTableIndex(rgba[0], rgba[1], rgba[2], rgba[3]);

                if (RgbaEquals(rgba[0], rgba[1], rgba[2], rgba[3], index[indexPos], index[indexPos + 1], index[indexPos + 2], index[indexPos + 3]))
                {
                    buffer[p++] = (byte)(QoiCodec.Index | (indexPos / 4));
                }
                else
                {
                    rgba.CopyTo(index.AsSpan(indexPos));

                    if (rgba[3] == prev[3])
                    {
                        int vr = rgba[0] - prev[0];
                        int vg = rgba[1] - prev[1];
                        int vb = rgba[2] - prev[2];

                        int vgr = vr - vg;
                        int vgb = vb - vg;

                        if (vr is > -3 and < 2 &&
                            vg is > -3 and < 2 &&
                            vb is > -3 and < 2)
                        {
                            counter++;
                            buffer[p++] = (byte)(QoiCodec.Diff | (vr + 2) << 4 | (vg + 2) << 2 | (vb + 2));
                        }
                        else if (vgr is > -9 and < 8 &&
                                 vg is > -33 and < 32 &&
                                 vgb is > -9 and < 8
                                )
                        {
                            buffer[p++] = (byte)(QoiCodec.Luma | (vg + 32));
                            buffer[p++] = (byte)((vgr + 8) << 4 | (vgb + 8));
                        }
                        else
                        {
                            buffer[p++] = QoiCodec.Rgb;
                            rgba.Slice(0, 3).CopyTo(buffer.Slice(p));
                            p += 3;
                        }
                    }
                    else
                    {
                        buffer[p++] = QoiCodec.Rgba;
                        rgba.CopyTo(buffer.Slice (p));
                        p += 4;
                    }
                }
            }
            prevAsInt[0] = rgbaAsInt[0];
        }

        QoiCodec.Padding.Span.CopyTo(buffer.Slice (p));
        p += QoiCodec.Padding.Length;

        return p;
    }

    private static bool RgbaEquals(byte r1, byte g1, byte b1, byte a1, byte r2, byte g2, byte b2, byte a2) =>
        r1 == r2 &&
        g1 == g2 &&
        b1 == b2 &&
        a1 == a2;
}