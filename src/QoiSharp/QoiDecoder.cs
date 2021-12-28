using System.Buffers;
using System.Runtime.CompilerServices;
using QoiSharp.Codec;
using QoiSharp.Exceptions;

namespace QoiSharp;

/// <summary>
/// QOI decoder.
/// </summary>
public static class QoiDecoder
{
    /// <summary>
    /// Decodes QOI data into raw pixel data.
    /// </summary>
    /// <param name="data">QOI data</param>
    /// <returns>Decoding result.</returns>
    /// <exception cref="QoiDecodingException">Thrown when data is invalid.</exception>
    public static QoiImage Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length < QoiCodec.HeaderSize + QoiCodec.Padding.Length)
        {
            throw new QoiDecodingException("File too short");
        }

        if (!QoiCodec.IsValidMagic(data[..4]))
        {
            throw new QoiDecodingException("Invalid file magic"); // TODO: add magic value
        }

        int width = data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7];
        int height = data[8] << 24 | data[9] << 16 | data[10] << 8 | data[11];
        byte channels = data[12];
        ColorSpace colorSpace = (ColorSpace)data[13];

        if (width == 0)
        {
            throw new QoiDecodingException($"Invalid width: {width}");
        }
        if (height == 0 || height >= QoiCodec.MaxPixels / width)
        {
            throw new QoiDecodingException($"Invalid height: {height}. Maximum for this image is {QoiCodec.MaxPixels / width - 1}");
        }

        byte[] pixels = new byte[width * height * channels];

        if (channels == 3) Decode3Channel(data.Slice(QoiCodec.HeaderSize), pixels);
        else if (channels == 4) Decode4Channel(data.Slice(QoiCodec.HeaderSize), pixels);
        else throw new QoiDecodingException($"Invalid number of channels: {channels}");

        int pixelsEnd = data.Length - QoiCodec.Padding.Length;
        for (int padIdx = 0; padIdx < QoiCodec.Padding.Length; padIdx++)
        {
            if (data[pixelsEnd + padIdx] != QoiCodec.Padding[padIdx])
            {
                throw new InvalidOperationException("Invalid padding");
            }
        }

        return new QoiImage(pixels, width, height, (Channels)channels, colorSpace);
    }

    /// <summary>
    /// Decode QOI image data of an RGB image
    /// </summary>
    /// <param name="data">encoded data</param>
    /// <param name="pixels">destination of decoded image data</param>
    /// <returns>used byte count inside <paramref name="pixels"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Decode3Channel(ReadOnlySpan<byte> data, Span<byte> pixels)
    {
        byte[] index = ArrayPool<byte>.Shared.Rent(QoiCodec.HashTableSize * 4);
        for (int indexPos = 3; indexPos < index.Length; indexPos += 4)
        {
            index[indexPos] = 255;
        }

        byte r = 0;
        byte g = 0;
        byte b = 0;

        int run = 0;
        int p = 0;

        for (int pxPos = 0; pxPos < pixels.Length; pxPos += 3)
        {
            if (run > 0)
            {
                run--;
            }
            else
            {
                byte b1 = data[p++];

                if (b1 == QoiCodec.Rgb)
                {
                    r = data[p++];
                    g = data[p++];
                    b = data[p++];
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Index)
                {
                    int indexPos = (b1 & ~QoiCodec.Mask2) * 4;
                    r = index[indexPos];
                    g = index[indexPos + 1];
                    b = index[indexPos + 2];
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Diff)
                {
                    r += (byte)(((b1 >> 4) & 0x03) - 2);
                    g += (byte)(((b1 >> 2) & 0x03) - 2);
                    b += (byte)((b1 & 0x03) - 2);
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Luma)
                {
                    int b2 = data[p++];
                    int vg = (b1 & 0x3F) - 32;
                    r += (byte)(vg - 8 + ((b2 >> 4) & 0x0F));
                    g += (byte)vg;
                    b += (byte)(vg - 8 + (b2 & 0x0F));
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Run)
                {
                    run = b1 & 0x3F;
                }

                int indexPos2 = QoiCodec.CalculateHashTableIndex(r, g, b);
                index[indexPos2] = r;
                index[indexPos2 + 1] = g;
                index[indexPos2 + 2] = b;
            }

            pixels[pxPos] = r;
            pixels[pxPos + 1] = g;
            pixels[pxPos + 2] = b;
        }

        ArrayPool<byte>.Shared.Return(index);
    }

    /// <summary>
    /// Decode QOI image data of an RGB image
    /// </summary>
    /// <param name="data">encoded data</param>
    /// <param name="pixels">destination of decoded image data</param>
    /// <returns>used byte count inside <paramref name="pixels"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Decode4Channel(ReadOnlySpan<byte> data, Span<byte> pixels)
    {
        byte[] index = ArrayPool<byte>.Shared.Rent(QoiCodec.HashTableSize * 4);

        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 255;

        int run = 0;
        int p = 0;

        for (int pxPos = 0; pxPos < pixels.Length; pxPos += 4)
        {
            if (run > 0)
            {
                run--;
            }
            else
            {
                byte b1 = data[p++];

                if (b1 == QoiCodec.Rgb)
                {
                    r = data[p++];
                    g = data[p++];
                    b = data[p++];
                }
                else if (b1 == QoiCodec.Rgba)
                {
                    r = data[p++];
                    g = data[p++];
                    b = data[p++];
                    a = data[p++];
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Index)
                {
                    int indexPos = (b1 & ~QoiCodec.Mask2) * 4;
                    r = index[indexPos];
                    g = index[indexPos + 1];
                    b = index[indexPos + 2];
                    a = index[indexPos + 3];
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Diff)
                {
                    r += (byte)(((b1 >> 4) & 0x03) - 2);
                    g += (byte)(((b1 >> 2) & 0x03) - 2);
                    b += (byte)((b1 & 0x03) - 2);
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Luma)
                {
                    int b2 = data[p++];
                    int vg = (b1 & 0x3F) - 32;
                    r += (byte)(vg - 8 + ((b2 >> 4) & 0x0F));
                    g += (byte)vg;
                    b += (byte)(vg - 8 + (b2 & 0x0F));
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Run)
                {
                    run = b1 & 0x3F;
                }

                int indexPos2 = QoiCodec.CalculateHashTableIndex(r, g, b, a);
                index[indexPos2] = r;
                index[indexPos2 + 1] = g;
                index[indexPos2 + 2] = b;
                index[indexPos2 + 3] = a;
            }

            pixels[pxPos] = r;
            pixels[pxPos + 1] = g;
            pixels[pxPos + 2] = b;
            pixels[pxPos + 3] = a;
        }

        ArrayPool<byte>.Shared.Return(index);
    }
}