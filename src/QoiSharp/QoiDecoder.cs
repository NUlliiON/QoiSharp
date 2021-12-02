using QoiSharp.Exceptions;

namespace QoiSharp;

/// <summary>
/// QOI decoder.
/// </summary>
public static class QoiDecoder 
{
    /// <summary>
    /// Decodes QOI data into pixel data.
    /// </summary>
    /// <param name="data">QOI data</param>
    /// <returns>Decoding result.</returns>
    /// <exception cref="QoiDecodingException">Thrown when data is invalid.</exception>
    public static QoiImage Decode(byte[] data)
    {
        if (data.Length < QoiCodec.HeaderSize + QoiCodec.Padding)
        {
            throw new QoiDecodingException("File too short");
        }

        int magic = data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3];
        if (magic != QoiCodec.Magic)
        {
            throw new QoiDecodingException($"Invalid file magic: 0x${magic:x8}");
        }

        int width = data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7];
        int height = data[8] << 24 | data[9] << 16 | data[10] << 8 | data[11];
        byte channels = data[12];
        var colorSpace = (ColorSpace)data[13];

        if (width == 0)
        {
            throw new QoiDecodingException($"Invalid width: {width}");
        }
        if (height == 0)
        {
            throw new QoiDecodingException($"Invalid height: {height}");
        }
        if (channels != 3 && channels != 4)
        {
            throw new QoiDecodingException($"Invalid number of channels: {channels}");
        }

        byte[] pixels = new byte[width * height * 4];
        byte[] index = new byte[4 * 64];
        if (channels == 3)
        {
            for (int indexPos = 3; indexPos < index.Length; indexPos += 4)
            {
                index[indexPos] = 255;
            }
        }

        int run = 0;
        // TODO: use struct instead?
        byte r = 255;
        byte g = 255;
        byte b = 255;
        byte a = 255;
        
        int chunksLength = data.Length - QoiCodec.Padding;
        int p = QoiCodec.HeaderSize;
        int pxLength = pixels.Length;
        
        for (int pxPos = 0; pxPos < pxLength; pxPos += 4)
        {
            if (run > 0)
            {
                run--;
            }
            else if (p < chunksLength)
            {
                byte b1 = data[p++];

                if ((b1 & QoiCodec.Mask2) == QoiCodec.Index)
                {
                    int indexPos = (b1 ^ QoiCodec.Index) * 4;
                    r = index[indexPos];
                    g = index[indexPos + 1];
                    b = index[indexPos + 2];
                    a = index[indexPos + 3];
                }
                else if ((b1 & QoiCodec.Mask3) == QoiCodec.Run8)
                {
                    run = b1 & 0x1f;
                }
                else if ((b1 & QoiCodec.Mask3) == QoiCodec.Run16)
                {
                    byte b2  = data[p++];
                    run = (((b1 & 0x1f) << 8) | b2) + 32;
                }
                else if ((b1 & QoiCodec.Mask2) == QoiCodec.Diff8)
                {
                    r += (byte)(((b1 >> 4) & 0x03) - 2);
                    g += (byte)(((b1 >> 2) & 0x03) - 2);
                    b += (byte)((b1 & 0x03) - 2);
                }
                else if ((b1 & QoiCodec.Mask3) == QoiCodec.Diff16)
                {
                    byte b2 = data[p++];
                    r += (byte)((b1 & 0x1f) - 16);
                    g += (byte)((b2 >> 4) - 8);
                    b += (byte)((b2 & 0x0f) - 8);
                }
                else if ((b1 & QoiCodec.Mask4) == QoiCodec.Diff24)
                {
                    byte b2 = data[p++];
                    byte b3 = data[p++];
                    r += (byte)((((b1 & 0x0f) << 1) | (b2 >> 7)) - 16);
                    g += (byte)(((b2 & 0x7c) >> 2) - 16);
                    b += (byte)((((b2 & 0x03) << 3) | ((b3 & 0xe0) >> 5)) - 16);
                    a += (byte)((b3 & 0x1f) - 16);
                }
                else if ((b1 & QoiCodec.Mask4) == QoiCodec.Color)
                {
                    if ((b1 & 8) != 0)
                    {
                        r = data[p++];
                    }
                    if ((b1 & 4) != 0)
                    {
                        g = data[p++];
                    }
                    if ((b1 & 2) != 0)
                    {
                        b = data[p++];
                    }
                    if ((b1 & 1) != 0)
                    {
                        a = data[p++];
                    }
                }

                int indexPos2 = ((r ^ g ^ b ^ a) % 64) * 4;
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

        return new QoiImage(pixels, width, height, channels, colorSpace);
    }
}