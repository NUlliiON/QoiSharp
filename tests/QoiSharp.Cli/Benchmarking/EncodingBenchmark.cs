using BenchmarkDotNet.Attributes;
using QoiSharp.Cli.Benchmarking.Configs;
using QoiSharp.Tests;

using StbImageSharp;

namespace QoiSharp.Cli.Benchmarking;

[MemoryDiagnoser]
public class EncodingBenchmark
{
    readonly ImageResult image;
    readonly byte[] rawData;
    readonly byte[] encoded;

    public EncodingBenchmark()
    {
        rawData = File.ReadAllBytes(Path.Combine (Constants.RootImagesDirectory, "photo_kodak", "kodim01.png"));
        image = ImageResult.FromMemory(rawData, ColorComponents.RedGreenBlueAlpha);
        encoded = new byte[image.Data.Length];
     }

    [Benchmark]
    public void QoiEncoding()
    {
        var qoi = new QoiImage (image.Data, image.Width, image.Height, image.SourceComp == ColorComponents.RedGreenBlueAlpha ? Codec.Channels.RgbWithAlpha : Codec.Channels.Rgb);
        QoiEncoder.Encode(qoi);
    }
}