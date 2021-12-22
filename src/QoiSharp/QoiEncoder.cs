using QoiSharp.Codec;
using QoiSharp.Exceptions;

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

        buffer[0] = (byte)(QoiCodec.Magic >> 24);
        buffer[1] = (byte)(QoiCodec.Magic >> 16);
        buffer[2] = (byte)(QoiCodec.Magic >> 8);
        buffer[3] = (byte)QoiCodec.Magic;

        buffer[4] = (byte)(width >> 24);
        buffer[5] = (byte)(width >> 16);
        buffer[6] = (byte)(width >> 8);
        buffer[7] = (byte)width;

        buffer[8] = (byte)(height >> 24);
        buffer[9] = (byte)(height >> 16);
        buffer[10] = (byte)(height >> 8);
        buffer[11] = (byte)height;

        buffer[12] = channels;
        buffer[13] = colorSpace;

        byte[] index = new byte[QoiCodec.HashTableSize * 4];

        byte prevR = 0;
        byte prevG = 0;
        byte prevB = 0;
        byte prevA = 255;

        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 255;

        int run = 0;
        int p = QoiCodec.HeaderSize;
        bool hasAlpha = channels == 4;

        int pixelsLength = width * height * channels;
        int pixelsEnd = pixelsLength - channels;
        int counter = 0;

        for (int pxPos = 0; pxPos < pixelsLength; pxPos += channels)
        {
            r = pixels[pxPos];
            g = pixels[pxPos + 1];
            b = pixels[pxPos + 2];
            if (hasAlpha)
            {
                a = pixels[pxPos + 3];
            }

            if (RgbaEquals(prevR, prevG, prevB, prevA, r, g, b, a))
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

                int indexPos = QoiCodec.CalculateHashTableIndex(r, g, b, a);

                if (RgbaEquals(r, g, b, a, index[indexPos], index[indexPos + 1], index[indexPos + 2], index[indexPos + 3]))
                {
                    buffer[p++] = (byte)(QoiCodec.Index | (indexPos / 4));
                }
                else
                {
                    index[indexPos] = r;
                    index[indexPos + 1] = g;
                    index[indexPos + 2] = b;
                    index[indexPos + 3] = a;

                    if (a == prevA)
                    {
                        int vr = r - prevR;
                        int vg = g - prevG;
                        int vb = b - prevB;

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
                            buffer[p++] = r;
                            buffer[p++] = g;
                            buffer[p++] = b;
                        }
                    }
                    else
                    {
                        buffer[p++] = QoiCodec.Rgba;
                        buffer[p++] = r;
                        buffer[p++] = g;
                        buffer[p++] = b;
                        buffer[p++] = a;
                    }
                }
            }

            prevR = r;
            prevG = g;
            prevB = b;
            prevA = a;
        }

        for (int padIdx = 0; padIdx < QoiCodec.Padding.Length; padIdx++)
        {
            buffer[p + padIdx] = QoiCodec.Padding[padIdx];
        }

        p += QoiCodec.Padding.Length;

        return p;
    }

    private static bool RgbaEquals(byte r1, byte g1, byte b1, byte a1, byte r2, byte g2, byte b2, byte a2) =>
        r1 == r2 &&
        g1 == g2 &&
        b1 == b2 &&
        a1 == a2;
}