using BenchmarkDotNet.Running;

using Microsoft.Extensions.DependencyInjection;
using QoiSharp.Cli.Commands;
using QoiSharp.Cli.Commands.Benchmarks;
using QoiSharp.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace QoiSharp.Cli
{
    public class Program
    {
        public static int Main(string[] args)
        {
            new QoiSharp.Cli.Benchmarking.EncodingBenchmark().QoiEncoding();
            var summary = BenchmarkRunner.Run(typeof(QoiSharp.Cli.Benchmarking.EncodingBenchmark));
            return 1;
        }
    }
}
