using BenchmarkDotNet.Attributes;
using QoiSharp.Cli.Benchmarking.Configs;
using QoiSharp.Codec;
using StbImageSharp;

namespace QoiSharp.Cli.Benchmarking;

[Config(typeof(ShortRunConfig))]
public class DecodingBenchmark
{
    [GlobalSetup]
    public async Task GlobalSetup()
    {
    }

    [Benchmark(Description = "PNG Decoding")]
    public void PngDecoding()
    {
        // byte[] data = ImageResult.FromMemory(_pngData, ColorComponents.RedGreenBlueAlpha).Data;
    }

    [Benchmark(Description = "QOI Decoding")]
    public void QoiDecoding()
    {
        // byte[] data = QoiDecoder.Decode(_qoiData).Data;
    }
}