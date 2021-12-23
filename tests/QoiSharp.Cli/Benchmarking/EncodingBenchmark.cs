using BenchmarkDotNet.Attributes;
using QoiSharp.Cli.Benchmarking.Configs;

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
        rawData = File.ReadAllBytes(@"C:\qoi_benchmark_suite\images\photo_kodak\kodim01.png");
        image = ImageResult.FromMemory(rawData, ColorComponents.RedGreenBlueAlpha);
        encoded = new byte[image.Data.Length];
     }


    [Benchmark(Description = "QOI Decoding")]
    public void QoiEncoding()
    {
        var qoi = QoiImage.FromMemory (image.Data, image.Width, image.Height, image.SourceComp == ColorComponents.RedGreenBlueAlpha ? Codec.Channels.RgbWithAlpha : Codec.Channels.Rgb);
        QoiEncoder.Encode(qoi, encoded);
    }
}