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
            var registrations = new ServiceCollection();
            var registrar = new TypeRegistrar(registrations);
            
            var app = new CommandApp(registrar);
            app.Configure(config =>
            {
                config.AddCommand<DecodingBenchmarkCommand>("benchmark-decoding");
                config.AddCommand<EncodeImageToQoiCommand>("encode-to-qoi");
            });
            
            return app.Run(args);
        }
    }
}
