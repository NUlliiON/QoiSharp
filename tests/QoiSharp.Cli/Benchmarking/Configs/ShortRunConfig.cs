using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace QoiSharp.Cli.Benchmarking.Configs;

public class ShortRunConfig : ManualConfig
{
    public ShortRunConfig()
    {
        AddJob(Job.ShortRun
            .WithPlatform(Platform.X64)
            .WithJit(Jit.RyuJit)
            .WithWarmupCount(1)
        );
        AddLogger(new ConsoleLogger());
        AddColumn(TargetMethodColumn.Method);
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Error);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(BaselineRatioColumn.RatioMean);
        AddDiagnoser(MemoryDiagnoser.Default);
    }
}