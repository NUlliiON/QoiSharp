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
    public static QoiImage Decode(byte[] data)
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
        var colorSpace = (ColorSpace)data[13];

        if (width == 0)
        {
            throw new QoiDecodingException($"Invalid width: {width}");
        }
        if (height == 0 || height >= QoiCodec.MaxPixels / width)
        {
            throw new QoiDecodingException($"Invalid height: {height}. Maximum for this image is {QoiCodec.MaxPixels / width - 1}");
        }
        if (channels is not 3 and not 4)
        {
            throw new QoiDecodingException($"Invalid number of channels: {channels}");
        }
        
        byte[] index = new byte[QoiCodec.HashTableSize * 4];
        if (channels == 3) // TODO: delete
        {
            for (int indexPos = 3; indexPos < index.Length; indexPos += 4)
            {
                index[indexPos] = 255;
            }
        }

        byte[] pixels = new byte[width * height * channels];
        
        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 255;
        
        int run = 0;
        int p = QoiCodec.HeaderSize;

        for (int pxPos = 0; pxPos < pixels.Length; pxPos += channels)
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
            if (channels == 4)
            {
                pixels[pxPos + 3] = a;
            }
        }
        
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
}