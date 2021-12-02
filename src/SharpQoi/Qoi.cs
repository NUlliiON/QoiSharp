using SharpQoi.Exceptions;

namespace SharpQoi;

public record struct QoiDescription(int Width, int Height, byte Channels, byte ColorSpace = 0);
public record QoiDecodingResult(byte[] Data, byte Channels, byte ColorSpace);

public static class Qoi
{
    public static byte[] Encode(byte[] pixels, QoiDescription description)
    {
        if (description.Width is 0 or > 0xFFFF)
        {
            throw new QoiEncodingException($"Invalid width: {description.Width}");
        }
        if (description.Height is 0 or > 0xFFFF)
        {
            throw new QoiEncodingException($"Invalid height: {description.Height}");
        }
        if (description.Channels is not 3 and not 4)
        {
            throw new QoiEncodingException($"Invalid number of channels: {description.Channels}");
        }
        if ((0xf0 & description.ColorSpace) != 0)
        {
            throw new QoiEncodingException($"Invalid color space: 0x${description.ColorSpace:x8}");
        }

        int width = description.Width;
        int height = description.Height;
        byte channels = description.Channels;
        byte colorSpace = description.ColorSpace;
        
        byte[] bytes = new byte[QoiConsts.HeaderSize + QoiConsts.Padding + width * height * (channels + 1)];

        bytes[0] = (byte)(QoiConsts.Magic >> 24);
        bytes[1] = (byte)((QoiConsts.Magic >> 16) & 0xFF);
        bytes[2] = (byte)((QoiConsts.Magic >> 8) & 0xFF);
        bytes[3] = (byte)(QoiConsts.Magic & 0xFF);

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

        int run = 0|0;
        // TODO: use struct instead?
        byte rPrev = 0|0;
        byte gPrev = 0|0;
        byte bPrev = 0|0;
        byte aPrev = 255|0;
        int vPrev = 255|0;
        
        // TODO: use struct instead?
        byte r = 0|0;
        byte g = 0|0;
        byte b = 0|0;
        byte a = 255|0;
        int v = 255|0;
        
        int p = QoiConsts.HeaderSize;

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
                    bytes[p++] = (byte)(QoiConsts.Run8 | run);
                }
                else
                {
                    run -= 33;
                    bytes[p++] = (byte)(QoiConsts.Run16 | run >> 8);
                    bytes[p++] = (byte)run;
                }

                run = 0;
            }

            if (v != vPrev)
            {
                int indexPos = (r ^ g ^ b ^ a) % 64;

                if (index[indexPos] == v)
                {
                    bytes[p++] = (byte)(QoiConsts.Index | indexPos);
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
                            bytes[p++] = (byte)(QoiConsts.Diff8 | ((vr + 2) << 4) | (vg + 2) << 2 | (vb + 2));
                        }
                        else if (va == 0 &&
                                 vr is > -17 and < 16 &&
                                 vg is > -9 and < 8 && 
                                 vb is > -9 and < 8)
                        {
                            bytes[p++] = (byte)(QoiConsts.Diff16 | (vr + 16));
                            bytes[p++] = (byte)((vg + 8) << 4 | (vb + 8));
                        }
                        else
                        {
                            bytes[p++] = (byte)(QoiConsts.Diff24 | (vr + 16) >> 1);
                            bytes[p++] = (byte)((vr + 16) << 7 | (vg + 16) << 2 | (vb + 16) >> 3);
                            bytes[p++] = (byte)((vb + 16) << 5 | (va + 16));
                        }
                    }
                    else
                    {
                        bytes[p++] = (byte)(QoiConsts.Color | (vr != 0 ? 8 : 0) | (vg != 0 ? 4 : 0) | (vb != 0 ? 2 : 0) | (va != 0 ? 1 : 0));
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

        p += QoiConsts.Padding;

        return bytes[..p];
    }

    public static QoiDecodingResult Decode(byte[] data)
    {
        if (data.Length < QoiConsts.HeaderSize + QoiConsts.Padding)
        {
            throw new QoiDecodingException("File too short");
        }

        int magic = data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3];
        if (magic != QoiConsts.Magic)
        {
            throw new QoiDecodingException($"Invalid file magic: 0x${magic:x8}");
        }

        int width = data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7];
        int height = data[8] << 24 | data[9] << 16 | data[10] << 8 | data[11];
        byte channels = data[12];
        byte colorSpace = data[13];

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
        if ((0xf0 & colorSpace) != 0)
        {
            throw new QoiDecodingException($"illegal color space: 0x${colorSpace:x8}");
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
        
        int chunksLength = data.Length - QoiConsts.Padding;
        int p = QoiConsts.HeaderSize;
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

                if ((b1 & QoiConsts.Mask2) == QoiConsts.Index)
                {
                    int indexPos = (b1 ^ QoiConsts.Index) * 4;
                    r = index[indexPos];
                    g = index[indexPos + 1];
                    b = index[indexPos + 2];
                    a = index[indexPos + 3];
                }
                else if ((b1 & QoiConsts.Mask3) == QoiConsts.Run8)
                {
                    run = b1 & 0x1f;
                }
                else if ((b1 & QoiConsts.Mask3) == QoiConsts.Run16)
                {
                    byte b2  = data[p++];
                    run = (((b1 & 0x1f) << 8) | b2) + 32;
                }
                else if ((b1 & QoiConsts.Mask2) == QoiConsts.Diff8)
                {
                    r += (byte)(((b1 >> 4) & 0x03) - 2);
                    g += (byte)(((b1 >> 2) & 0x03) - 2);
                    b += (byte)((b1 & 0x03) - 2);
                }
                else if ((b1 & QoiConsts.Mask3) == QoiConsts.Diff16)
                {
                    byte b2 = data[p++];
                    r += (byte)((b1 & 0x1f) - 16);
                    g += (byte)((b2 >> 4) - 8);
                    b += (byte)((b2 & 0x0f) - 8);
                }
                else if ((b1 & QoiConsts.Mask4) == QoiConsts.Diff24)
                {
                    byte b2 = data[p++];
                    byte b3 = data[p++];
                    r += (byte)((((b1 & 0x0f) << 1) | (b2 >> 7)) - 16);
                    g += (byte)(((b2 & 0x7c) >> 2) - 16);
                    b += (byte)((((b2 & 0x03) << 3) | ((b3 & 0xe0) >> 5)) - 16);
                    a += (byte)((b3 & 0x1f) - 16);
                }
                else if ((b1 & QoiConsts.Mask4) == QoiConsts.Color)
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

        return new QoiDecodingResult(pixels, channels, colorSpace);
    }
}