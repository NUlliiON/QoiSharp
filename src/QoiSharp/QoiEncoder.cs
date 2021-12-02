using QoiSharp.Exceptions;

namespace QoiSharp;

/// <summary>
/// QOI encoder.
/// </summary>
public static class QoiEncoder
{
    /// <summary>
    /// Encodes pixel data into QOI.
    /// </summary>
    /// <param name="pixels">QOI image.</param>
    /// <returns>Encoded image.</returns>
    /// <exception cref="QoiEncodingException">Thrown when image information is invalid.</exception>
    public static byte[] Encode(QoiImage image)
    {
        if (image.Width is 0 or > 0xFFFF)
        {
            throw new QoiEncodingException($"Invalid width: {image.Width}");
        }
        if (image.Height is 0 or > 0xFFFF)
        {
            throw new QoiEncodingException($"Invalid height: {image.Height}");
        }
        if (image.Channels is not 3 and not 4)
        {
            throw new QoiEncodingException($"Invalid number of channels: {image.Channels}");
        }

        int width = image.Width;
        int height = image.Height;
        byte channels = image.Channels;
        byte colorSpace = (byte)image.ColorSpace;
        byte[] pixels = image.Pixels;
        
        byte[] bytes = new byte[QoiCodec.HeaderSize + QoiCodec.Padding + width * height * (channels + 1)];

        bytes[0] = (byte)(QoiCodec.Magic >> 24);
        bytes[1] = (byte)((QoiCodec.Magic >> 16) & 0xFF);
        bytes[2] = (byte)((QoiCodec.Magic >> 8) & 0xFF);
        bytes[3] = (byte)(QoiCodec.Magic & 0xFF);

        bytes[4] = (byte)(width >> 24);
        bytes[5] = (byte)((width >> 16) & 0xFF);
        bytes[6] = (byte)((width >> 8) & 0xFF);
        bytes[7] = (byte)(width & 0xFF);

        bytes[8] = (byte)(height >> 24);
        bytes[9] = (byte)((height >> 16) & 0xFF);
        bytes[10] = (byte)((height >> 8) & 0xFF);
        bytes[11] = (byte)(height & 0xFF);

        bytes[12] = channels;
        bytes[13] = colorSpace;

        int[] index = new int[64];

        int run = 0;
        // TODO: use struct instead?
        byte rPrev = 0;
        byte gPrev = 0;
        byte bPrev = 0;
        byte aPrev = 255;
        int vPrev = 255;
        
        // TODO: use struct instead?
        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 255;
        int v = 255;
        
        int p = QoiCodec.HeaderSize;

        int pxLength = width * height * 4;
        int pxEnd = pxLength - 4;

        for (int pxPos = 0; pxPos < pxLength; pxPos += channels)
        {
            r = pixels[pxPos];
            g = pixels[pxPos + 1];
            b = pixels[pxPos + 2];

            if (channels == 4) 
            {
                a = pixels[pxPos + 3];
            }

            v = (r << 24) | (g << 16) | (b << 8) | a;

            if (v == vPrev)
            {
                run++;
            }

            if (run > 0 && (run == 0x2020 || v != vPrev || pxPos == pxEnd))
            {
                if (run < 33)
                {
                    run -= 1;
                    bytes[p++] = (byte)(QoiCodec.Run8 | run);
                }
                else
                {
                    run -= 33;
                    bytes[p++] = (byte)(QoiCodec.Run16 | run >> 8);
                    bytes[p++] = (byte)run;
                }

                run = 0;
            }

            if (v != vPrev)
            {
                int indexPos = (r ^ g ^ b ^ a) % 64;

                if (index[indexPos] == v)
                {
                    bytes[p++] = (byte)(QoiCodec.Index | indexPos);
                }
                else
                {
                    index[indexPos] = v;

                    int vr = r - rPrev;
                    int vg = g - gPrev;
                    int vb = b - bPrev;
                    int va = a - aPrev;

                    if (vr is > -17 and < 16 && 
                        vg is > -17 and < 16 && 
                        vb is > -17 and < 16 && 
                        va is > -17 and < 16)
                    {
                        if (va == 0 &&
                            vr is > -3 and < 2 &&
                            vg is > -3 and < 2 &&
                            vb is > -3 and < 2)
                        {
                            bytes[p++] = (byte)(QoiCodec.Diff8 | ((vr + 2) << 4) | (vg + 2) << 2 | (vb + 2));
                        }
                        else if (va == 0 &&
                                 vg is > -9 and < 8 && 
                                 vb is > -9 and < 8)
                        {
                            bytes[p++] = (byte)(QoiCodec.Diff16 | (vr + 16));
                            bytes[p++] = (byte)((vg + 8) << 4 | (vb + 8));
                        }
                        else
                        {
                            bytes[p++] = (byte)(QoiCodec.Diff24 | (vr + 16) >> 1);
                            bytes[p++] = (byte)((vr + 16) << 7 | (vg + 16) << 2 | (vb + 16) >> 3);
                            bytes[p++] = (byte)((vb + 16) << 5 | (va + 16));
                        }
                    }
                    else
                    {
                        bytes[p++] = (byte)(QoiCodec.Color | (vr != 0 ? 8 : 0) | (vg != 0 ? 4 : 0) | (vb != 0 ? 2 : 0) | (va != 0 ? 1 : 0));
                        if (vr != 0)
                        {
                            bytes[p++] = r;
                        }
                        if (vg != 0)
                        {
                            bytes[p++] = g;
                        }
                        if (vb != 0)
                        {
                            bytes[p++] = b;
                        }
                        if (va != 0)
                        {
                            bytes[p++] = a;
                        }
                    }
                }
            }

            rPrev = r;
            gPrev = g;
            bPrev = b;
            aPrev = a;
            vPrev = v;
        }

        p += QoiCodec.Padding;

        return bytes[..p];
    }
}