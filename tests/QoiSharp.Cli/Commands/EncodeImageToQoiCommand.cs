using System.Diagnostics;
using QoiSharp.Codec;
using Spectre.Console;
using Spectre.Console.Cli;
using StbImageSharp;

namespace QoiSharp.Cli.Commands;

public class EncodeImageToQoiCommand : AsyncCommand<EncodeImageToQoiCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[InputImagePath]")]
        public string InputImagePath { get; init; } = null!;

        [CommandArgument(1, "[OutputQoiPath]")]
        public string OutputQoiPath { get; init; } = null!;

        public override ValidationResult Validate() => this switch
        {
            _ when string.IsNullOrWhiteSpace(InputImagePath) => ValidationResult.Error("Input path to 'IMAGE' cannot be empty."),
            _ when string.IsNullOrWhiteSpace(OutputQoiPath) => ValidationResult.Error("Output path to 'QOI' cannot be empty."),
            _ when !File.Exists(InputImagePath) => ValidationResult.Error("Input 'IMAGE' file not found."),
            _ => ValidationResult.Success()
        };
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var data = await File.ReadAllBytesAsync(settings.InputImagePath);
        var image = ImageResult.FromMemory(data);
        var stopwatch = new Stopwatch();

        AnsiConsole.Markup("Encoding... ");
        stopwatch.Start();
        byte[] qoiData = QoiEncoder.Encode(new QoiImage(image.Data, image.Width, image.Height, (Channels)image.Comp));
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[green]DONE[/] [yellow]({stopwatch.ElapsedMilliseconds}ms)[/]");
        
        AnsiConsole.Markup("Saving... ");
        await File.WriteAllBytesAsync(settings.OutputQoiPath, qoiData);
        AnsiConsole.MarkupLine("[green]DONE[/]");
        
        return 0;
    }
}