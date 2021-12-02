using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using StbImageSharp;
using Xunit;

namespace QoiSharp.Tests;

public class EncodingTests
{
    [Fact]
    public async Task PngEncodingShouldWorking()
    {
        // Arrange
        const string pngFileName = "Resources\\Images\\file_example_PNG_500kB.png";
        byte[] pngData = ImageResult.FromMemory(await File.ReadAllBytesAsync(pngFileName), ColorComponents.RedGreenBlueAlpha).Data;
        byte channels = 3;
        for (int index = 3; index < pngData.Length; index += 4)
        {
            if (pngData[index] == 0xFF) continue;
            
            channels = 4;
            break;
        }

        // Act
        byte[] qoiData = QoiEncoder.Encode(new QoiImage(pngData, 850, 566, channels: 4));
        
        // Assert
        qoiData.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task QoiDecodingShouldWorking()
    {
        // Arrange
        const string qoiFileName = "Resources\\Images\\file_example_PNG_500kB.qoi";
        byte[] qoiData = await File.ReadAllBytesAsync(qoiFileName);
        
        // Act
        var qoiImage = QoiDecoder.Decode(qoiData);

        // Assert
        const string pngFileName = "Resources\\Images\\file_example_PNG_500kB.png";
        byte[] pngData = ImageResult.FromMemory(await File.ReadAllBytesAsync(pngFileName), ColorComponents.RedGreenBlueAlpha).Data;
        Assert.True(qoiImage.Pixels.SequenceEqual(pngData));
        qoiImage.Channels.Should().Be(3);
        qoiImage.ColorSpace.Should().Be(0);
    }

    private void SomeExperiments()
    {
        // byte channels = 3;
        // for (int index = 3; index < pixels.Length; index += 4) 
        // {
        //     if (pixels[index] != 0xFF) 
        //     {
        //         channels = 4;
        //         break;
        //     }
        // }
        
        // string imgFileName = "Resources\\Images\\file_example_PNG_500kB.png";
        // var img = await Image.LoadAsync(imgFileName);
        // var ms = new MemoryStream();
        // await img.SaveAsync(ms, PngFormat.Instance);

        // var img = Image.FromFile(imgFileName);
        // var ms = new MemoryStream();
        // img.Save(ms, ImageFormat.Png);
        // byte[] pixels = ms.ToArray();

        // var pixels = ImageResult.FromMemory(await File.ReadAllBytesAsync(imgFileName), ColorComponents.RedGreenBlueAlpha).Data;
        // var img = await Image.LoadAsync(imgFileName, new PngDecoder());
        // var ms = new MemoryStream();
        // await img.SaveAsync(ms, new PngEncoder() {ColorType = PngColorType.RgbWithAlpha});
        // byte[] pixels = ms.ToArray();

        // byte channels = 3;
        // for (int index = 3; index < pixels.Length; index += 4)
        // {
        //     if (pixels[index] == 0xFF) continue;
        //     
        //     channels = 4;
        //     break;
        // }
        //
        //
        
        // var decodeResult = Qoi.Decode(encoded);

        // var img = await Image.LoadAsync(imgFileName);
        // var metadata = img.Metadata.GetPngMetadata();
        // byte channels = (byte)(metadata.ColorType == PngColorType.Rgb ? 3 : 4);
        // var imgMs = new MemoryStream();
        // if (channels == 3)
        // {
        //     await img.SaveAsPngAsync("1.png");
        //     img.Dispose();
        //
        //     img = await Image.LoadAsync("1.png");
        //     metadata = img.Metadata.GetPngMetadata();
        //     channels = (byte)(metadata.ColorType == PngColorType.Rgb ? 3 : 4);
        // }
        // else
        // {
        //     await img.SaveAsPngAsync(imgMs);
        // }

        // byte[] imgData = imgMs.ToArray();
        //
        // byte[] encoded = Qoi.Encode(imgData, new QoiDescription(880, 738, channels));
    }
}