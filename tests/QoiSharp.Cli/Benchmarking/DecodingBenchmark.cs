using BenchmarkDotNet.Attributes;
using QoiSharp.Cli.Benchmarking.Configs;
using QoiSharp.Codec;
using StbImageSharp;

namespace QoiSharp.Cli.Benchmarking;

[Config(typeof(ShortRunConfig))]
public class DecodingBenchmark
{
}