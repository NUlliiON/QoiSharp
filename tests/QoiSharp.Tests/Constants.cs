using System.IO;

namespace QoiSharp.Tests;

public static class Constants
{
    /// <remarks>
    /// Images are taken from here (size ~ 1.1GB): https://qoiformat.org/benchmark/qoi_benchmark_suite.tar
    /// </remarks>
    public static readonly string RootImagesDirectory;

    static Constants()
    {
        var currentDir = typeof(Constants).Assembly.Location;
        while (!string.IsNullOrEmpty(currentDir) && RootImagesDirectory == null)
        {
            if (Directory.Exists(Path.Combine(currentDir, "qoi_benchmark_suite", "images")))
                RootImagesDirectory = Path.Combine(currentDir, "qoi_benchmark_suite", "images");
            currentDir = Path.GetDirectoryName(currentDir);
        }

        if (RootImagesDirectory == null)
            throw new System.Exception("You must decompress the 'qoi_benchmark_suite' archive and place it in a directory above this one.");
    }

    public class Images
    {
        public const string PhotoTecnickSubDirectory = @"photo_tecnick";
        public const string TexturesPhotoSubDirectory = @"textures_photo";
        public const string PngImgSubDirectory = @"pngimg";
        public const string PhotoWikipediaSubDirectory = @"photo_wikipedia";

        public static string GetFullPath(string relative)
            => Path.Combine(RootImagesDirectory, relative);
    }
}