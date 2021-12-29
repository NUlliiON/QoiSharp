using BenchmarkDotNet.Attributes;

using QoiSharp.Cli.Benchmarking.Configs;
using QoiSharp.Tests;

using StbImageSharp;

namespace QoiSharp.Cli.Benchmarking;

[MemoryDiagnoser]
public class DecodingBenchmark
{
    readonly byte[] encoded;

    public DecodingBenchmark()
    {
        var rawData = File.ReadAllBytes(Path.Combine(Constants.RootImagesDirectory, "photo_kodak", "kodim01.png"));
        var image = ImageResult.FromMemory(rawData, ColorComponents.RedGreenBlueAlpha);
        var qoi = new QoiImage(image.Data, image.Width, image.Height, image.Comp == ColorComponents.RedGreenBlueAlpha ? Codec.Channels.RgbWithAlpha : Codec.Channels.Rgb);
        encoded = QoiEncoder.Encode(qoi);
    }

    [Benchmark]
    public void QoiDecoding()
        => QoiDecoder.Decode(encoded);
}