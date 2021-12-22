using BenchmarkDotNet.Running;
using QoiSharp.Cli.Benchmarking;
using Spectre.Console;
using Spectre.Console.Cli;

namespace QoiSharp.Cli.Commands.Benchmarks
{
    public sealed class DecodingBenchmarkCommand : Command<DecodingBenchmarkCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "[ImagesDirectoryPath]")]
            public string ImagesDirectoryPath { get; set; } = null!;
            
            [CommandArgument(1, "[ImagesCount]")]
            public int ImagesCount { get; set; }

            public override ValidationResult Validate() => this switch
            {
                _ when string.IsNullOrWhiteSpace(ImagesDirectoryPath) => ValidationResult.Error("Directory path to images cannot be empty."),
                _ when !Directory.Exists(ImagesDirectoryPath) => ValidationResult.Error("Directory not found"),
                _ when ImagesCount is > 1000 or < 1 => ValidationResult.Error("The number must be in the range from 1 to 1000"),
                _ => ValidationResult.Success()
            };
        }
        
        
        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                ExecuteInternal(context, settings);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(
                    ex,
                    ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes |
                    ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
                return 1;
            }

            return 0;
        }

        private int ExecuteInternal(CommandContext context, Settings settings)
        {
            string? pngImagePath = Directory.EnumerateFiles(settings.ImagesDirectoryPath, "*.png").FirstOrDefault();
            if (pngImagePath is null)
            {
                throw new InvalidOperationException("Image not found");
            }
            // DecodingBenchmark.Arguments.PngImagePath = pngImagePath;
            _ = BenchmarkRunner.Run<DecodingBenchmark>();

            return 0;
        }
    }
}
