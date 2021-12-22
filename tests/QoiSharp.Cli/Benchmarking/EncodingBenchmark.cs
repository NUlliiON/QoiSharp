using BenchmarkDotNet.Attributes;
using QoiSharp.Cli.Benchmarking.Configs;

namespace QoiSharp.Cli.Benchmarking;

[Config(typeof(ShortRunConfig))]
public class EncodingBenchmark
{
   
}