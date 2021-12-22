namespace QoiSharp.Tests;

public static class Constants
{
    /// <remarks>
    /// Images are taken from here (size ~ 1.1GB): https://qoiformat.org/benchmark/qoi_benchmark_suite.tar
    /// </remarks>
    public const string RootImagesDirectory = @"E:\Tests\qoi_benchmark_suite\images";

    public class Images
    {
        public const string PhotoTecnick = $@"{RootImagesDirectory}\photo_tecnick";
        public const string TexturesPhoto = $@"{RootImagesDirectory}\textures_photo";
        public const string PngImg = $@"{RootImagesDirectory}\pngimg";
        public const string PhotoWikipedia = $@"{RootImagesDirectory}\photo_wikipedia";
    }
}